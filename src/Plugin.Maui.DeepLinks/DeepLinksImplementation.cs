namespace Plugin.Maui.DeepLinks;

sealed class DeepLinksImplementation : IDeepLinks
{
    readonly object _gate = new();
    readonly List<MappedRoute> _maps = [];
    readonly Queue<DeepLink> _queue = new();
    readonly IDeepLinkStore _store;
    readonly IDeepLinkNavigator _navigator;
    readonly IDeepLinkAuthenticator? _authenticator;
    readonly IClock _clock;
    IDeepLinkLogger? _logger;
    bool _logging;
    bool _ready;
    string? _lastUri;
    DateTimeOffset _lastHandledAt;

    internal DeepLinksImplementation(
        DeepLinksOptions options,
        IDeepLinkStore store,
        IDeepLinkNavigator navigator,
        IDeepLinkAuthenticator? authenticator,
        IClock clock)
    {
        Options = options ?? throw new ArgumentNullException(nameof(options));
        _store = store ?? throw new ArgumentNullException(nameof(store));
        _navigator = navigator ?? throw new ArgumentNullException(nameof(navigator));
        _authenticator = authenticator;
        _clock = clock ?? throw new ArgumentNullException(nameof(clock));
        _logging = options.EnableLogging;
        _logger = options.Logger;

        foreach (var map in options.Maps)
        {
            AddMap(map.Template, map.Handler, map.ShellRoute, map.Options);
        }
    }

    public bool IsSupported => true;

    public DeepLinksPlatformInfo Platform { get; } = DeepLinksPlatformInfo.Current;

    public DeepLinksOptions Options { get; }

    public bool IsReady
    {
        get
        {
            lock (_gate)
            {
                return _ready;
            }
        }
    }

    public DeepLink? Pending
    {
        get
        {
            var pending = _store.LoadPending();
            return IsExpired(pending) ? null : pending;
        }
    }

    public bool HasSavedNavigationStack => _store.LoadStack() is { HasLocation: true };

    public event EventHandler<DeepLinkReceivedEventArgs>? Received;
    public event EventHandler<DeepLinkNavigatedEventArgs>? Navigated;
    public event EventHandler<DeepLinkDeferredEventArgs>? Deferred;
    public event EventHandler<DeepLinkUnhandledEventArgs>? Unhandled;
    public event EventHandler<DeepLinkFailedEventArgs>? Failed;

    public IDeepLinks Map(string template, Func<DeepLinkRoute, Task> handler) =>
        Map(template, handler, new DeepLinkMapOptions());

    public IDeepLinks Map(string template, Func<DeepLinkRoute, Task> handler, bool requiresAuthentication) =>
        Map(template, handler, new DeepLinkMapOptions { RequiresAuthentication = requiresAuthentication });

    public IDeepLinks Map(string template, Func<DeepLinkRoute, Task> handler, DeepLinkMapOptions options)
    {
        ArgumentNullException.ThrowIfNull(handler);
        return Map(template, (route, _) => handler(route), options);
    }

    public IDeepLinks Map(string template, Func<DeepLinkRoute, CancellationToken, Task> handler, DeepLinkMapOptions? options = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(template);
        ArgumentNullException.ThrowIfNull(handler);
        AddMap(template, handler, shellRoute: null, options ?? new DeepLinkMapOptions());
        return this;
    }

    public IDeepLinks Map(string template, Action<DeepLinkRoute> handler, DeepLinkMapOptions? options = null)
    {
        ArgumentNullException.ThrowIfNull(handler);
        return Map(template, (route, _) =>
        {
            handler(route);
            return Task.CompletedTask;
        }, options);
    }

    public IDeepLinks Map(string template, string shellRoute, bool requiresAuthentication = false)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(template);
        ArgumentException.ThrowIfNullOrWhiteSpace(shellRoute);
        AddMap(template, handler: null, shellRoute, new DeepLinkMapOptions { RequiresAuthentication = requiresAuthentication });
        return this;
    }

    public bool Unmap(string template)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(template);
        var normalized = RouteMatcher.NormalizePath(template);
        lock (_gate)
        {
            return _maps.RemoveAll(map =>
                string.Equals(map.Pattern.Normalized, normalized, StringComparison.OrdinalIgnoreCase)) > 0;
        }
    }

    public Task<DeepLinkResult> HandleAsync(string uri, DeepLinkLaunch launch = DeepLinkLaunch.Warm, CancellationToken cancellationToken = default)
    {
        if (!DeepLinkParser.TryParse(uri, launch, Options, out var link))
        {
            return Task.FromResult(DeepLinkResult.ForIgnored());
        }

        return HandleAsync(link, cancellationToken);
    }

    public Task<DeepLinkResult> HandleAsync(Uri uri, DeepLinkLaunch launch = DeepLinkLaunch.Warm, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(uri);
        if (!DeepLinkParser.TryParse(uri, launch, Options, native: null, out var link))
        {
            return Task.FromResult(DeepLinkResult.ForIgnored());
        }

        return HandleAsync(link, cancellationToken);
    }

    public async Task<DeepLinkResult> HandleAsync(DeepLink link, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(link);

        if (IsDuplicate(link))
        {
            Log(DeepLinkLogLevel.Debug, $"Ignored duplicate {link.Uri}");
            return DeepLinkResult.ForIgnored(link);
        }

        Received?.Invoke(this, new DeepLinkReceivedEventArgs(link));

        if (Options.QueueUntilReady && !IsReady)
        {
            Enqueue(link, persist: Options.PersistPendingLinks && Options.PersistNotReadyLinks);
            var deferred = DeepLinkResult.ForDeferred(link, DeepLinkDeferralReason.NotReady);
            Deferred?.Invoke(this, new DeepLinkDeferredEventArgs(link, DeepLinkDeferralReason.NotReady));
            Log(DeepLinkLogLevel.Debug, $"Queued until ready: {link.Uri}");
            return deferred;
        }

        return await DispatchCoreAsync(link, cancellationToken).ConfigureAwait(false);
    }

    public void Handle(string uri, DeepLinkLaunch launch = DeepLinkLaunch.Warm) =>
        FireAndForget(() => HandleAsync(uri, launch));

    public void Handle(Uri uri, DeepLinkLaunch launch = DeepLinkLaunch.Warm) =>
        FireAndForget(() => HandleAsync(uri, launch));

    public void Handle(DeepLink link) =>
        FireAndForget(() => HandleAsync(link));

    public void MarkReady()
    {
        List<DeepLink> pending;
        lock (_gate)
        {
            _ready = true;
            pending = [.. _queue];
            _queue.Clear();
        }

        Log(DeepLinkLogLevel.Information, $"Ready. Flushing {pending.Count} queued link(s).");

        foreach (var link in pending)
        {
            FireAndForget(() => DispatchCoreAsync(link, CancellationToken.None));
        }

        var stored = _store.LoadPending();
        if (stored is not null && pending.TrueForAll(item => !SameUri(item, stored)))
        {
            FireAndForget(() => ResumeStoredAsync(stored, CancellationToken.None));
        }
    }

    public async Task<DeepLinkResult?> NotifyAuthenticatedAsync(CancellationToken cancellationToken = default)
    {
        var pending = _store.LoadPending();
        if (pending is null)
        {
            Log(DeepLinkLogLevel.Debug, "NotifyAuthenticated: no pending link.");
            return null;
        }

        if (IsExpired(pending))
        {
            _store.ClearPending();
            Log(DeepLinkLogLevel.Information, "NotifyAuthenticated: pending link expired.");
            return DeepLinkResult.ForIgnored(pending);
        }

        Log(DeepLinkLogLevel.Information, $"NotifyAuthenticated: restoring {pending.Uri}");
        return await DispatchCoreAsync(pending, cancellationToken).ConfigureAwait(false);
    }

    public void NotifyAuthenticated() =>
        FireAndForget(async () => await NotifyAuthenticatedAsync().ConfigureAwait(false));

    public void NotifyUnauthenticated() =>
        Log(DeepLinkLogLevel.Debug, "User signed out. Pending link retained.");

    public bool ClearPending()
    {
        var had = _store.LoadPending() is not null;
        _store.ClearPending();
        lock (_gate)
        {
            _queue.Clear();
        }

        return had;
    }

    public async Task RestoreNavigationStackAsync(CancellationToken cancellationToken = default)
    {
        var snapshot = _store.LoadStack();
        if (snapshot is null || !snapshot.HasLocation)
        {
            return;
        }

        await _navigator.RestoreStackAsync(snapshot, cancellationToken).ConfigureAwait(false);
        Log(DeepLinkLogLevel.Information, $"Restored navigation stack {snapshot.Location}");
    }

    public void EnableLogging(bool enabled, IDeepLinkLogger? logger = null)
    {
        _logging = enabled;
        if (logger is not null)
        {
            _logger = logger;
        }
        else if (enabled)
        {
            _logger ??= new DebugDeepLinkLogger();
        }
    }

    void AddMap(
        string template,
        Func<DeepLinkRoute, CancellationToken, Task>? handler,
        string? shellRoute,
        DeepLinkMapOptions options)
    {
        var pattern = RouteMatcher.Compile(template, Options.CaseInsensitivePaths);
        lock (_gate)
        {
            _maps.RemoveAll(map =>
                string.Equals(map.Pattern.Normalized, pattern.Normalized, StringComparison.OrdinalIgnoreCase));
            _maps.Add(new MappedRoute(pattern, handler, shellRoute, options));
        }

        Log(DeepLinkLogLevel.Debug, $"Mapped {pattern.Normalized}");
    }

    async Task<DeepLinkResult> DispatchCoreAsync(DeepLink link, CancellationToken cancellationToken)
    {
        if (!TryMatch(link, out var mapped, out var route))
        {
            Unhandled?.Invoke(this, new DeepLinkUnhandledEventArgs(link));
            Log(DeepLinkLogLevel.Warning, $"Unmatched {link.Uri} path={link.Path}");
            return DeepLinkResult.ForUnmatched(link);
        }

        if (RequiresAuth(mapped) && !IsUserAuthenticated())
        {
            PersistPending(link);
            await CaptureStackIfNeededAsync(mapped, cancellationToken).ConfigureAwait(false);

            if (!string.IsNullOrWhiteSpace(Options.LoginPath) && _navigator.CanNavigate)
            {
                try
                {
                    await _navigator.NavigateAsync(Options.LoginPath, route, cancellationToken).ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    Log(DeepLinkLogLevel.Error, $"Failed to open login path '{Options.LoginPath}'.", ex);
                    Failed?.Invoke(this, new DeepLinkFailedEventArgs(link, ex, route));
                    return DeepLinkResult.ForFailed(link, ex, route);
                }
            }

            Deferred?.Invoke(this, new DeepLinkDeferredEventArgs(link, DeepLinkDeferralReason.AuthenticationRequired, route));
            Log(DeepLinkLogLevel.Information, $"Awaiting authentication for {link.Uri}");
            return DeepLinkResult.ForDeferred(link, DeepLinkDeferralReason.AuthenticationRequired, route);
        }

        try
        {
            await CaptureStackIfNeededAsync(mapped, cancellationToken).ConfigureAwait(false);

            if (mapped.Handler is not null)
            {
                await InvokeHandlerAsync(mapped.Handler, route, cancellationToken).ConfigureAwait(false);
            }

            string? navigationRoute = null;
            if (mapped.Handler is null && !string.IsNullOrWhiteSpace(mapped.ShellRoute))
            {
                navigationRoute = ShellRouteExpander.Expand(mapped.ShellRoute, route);
                if (!_navigator.CanNavigate)
                {
                    throw new InvalidOperationException(
                        "The navigator is not ready. Call MarkReady() after Shell is created.");
                }

                await _navigator.NavigateAsync(navigationRoute, route, cancellationToken).ConfigureAwait(false);
            }

            _store.ClearPending();
            var result = DeepLinkResult.ForNavigated(link, route, navigationRoute);
            Navigated?.Invoke(this, new DeepLinkNavigatedEventArgs(result));
            Log(DeepLinkLogLevel.Information, $"Navigated {link.Uri} -> {mapped.Pattern.Template}");
            return result;
        }
        catch (Exception ex)
        {
            Log(DeepLinkLogLevel.Error, $"Dispatch failed for {link.Uri}.", ex);
            Failed?.Invoke(this, new DeepLinkFailedEventArgs(link, ex, route));
            return DeepLinkResult.ForFailed(link, ex, route);
        }
    }

    async Task<DeepLinkResult> ResumeStoredAsync(DeepLink stored, CancellationToken cancellationToken)
    {
        if (IsExpired(stored))
        {
            _store.ClearPending();
            return DeepLinkResult.ForIgnored(stored);
        }

        if (!TryMatch(stored, out var mapped, out _))
        {
            return await DispatchCoreAsync(stored, cancellationToken).ConfigureAwait(false);
        }

        if (RequiresAuth(mapped) && !IsUserAuthenticated())
        {
            return DeepLinkResult.ForDeferred(stored, DeepLinkDeferralReason.AuthenticationRequired);
        }

        return await DispatchCoreAsync(stored, cancellationToken).ConfigureAwait(false);
    }

    bool TryMatch(DeepLink link, [NotNullWhen(true)] out MappedRoute? mapped, [NotNullWhen(true)] out DeepLinkRoute? route)
    {
        mapped = null;
        route = null;
        MappedRoute? best = null;
        Dictionary<string, string>? bestValues = null;

        lock (_gate)
        {
            foreach (var candidate in _maps)
            {
                if (!candidate.Pattern.TryMatch(link.Path, out var values))
                {
                    continue;
                }

                if (best is null || RouteMatcher.CompareSpecificity(candidate.Pattern, best.Pattern) < 0)
                {
                    best = candidate;
                    bestValues = values;
                }
            }
        }

        if (best is null || bestValues is null)
        {
            return false;
        }

        mapped = best;
        route = new DeepLinkRoute(link, best.Pattern.Template, bestValues, RequiresAuth(best));
        return true;
    }

    bool RequiresAuth(MappedRoute mapped) =>
        mapped.Options.RequiresAuthentication || Options.RequireAuthenticationByDefault;

    bool IsUserAuthenticated() => _authenticator?.IsAuthenticated ?? true;

    async Task CaptureStackIfNeededAsync(MappedRoute mapped, CancellationToken cancellationToken)
    {
        var capture = mapped.Options.CaptureNavigationStack ?? Options.CaptureNavigationStack;
        if (!capture)
        {
            return;
        }

        var snapshot = await _navigator.CaptureStackAsync(cancellationToken).ConfigureAwait(false);
        if (snapshot is not null)
        {
            _store.SaveStack(snapshot);
        }
    }

    async Task InvokeHandlerAsync(
        Func<DeepLinkRoute, CancellationToken, Task> handler,
        DeepLinkRoute route,
        CancellationToken cancellationToken)
    {
#if ANDROID || IOS
        try
        {
            if (!MainThread.IsMainThread)
            {
                await MainThread.InvokeOnMainThreadAsync(() => handler(route, cancellationToken)).ConfigureAwait(false);
                return;
            }
        }
        catch (InvalidOperationException)
        {
            // No UI synchronization context (tests).
        }
#endif
        await handler(route, cancellationToken).ConfigureAwait(false);
    }

    void Enqueue(DeepLink link, bool persist)
    {
        lock (_gate)
        {
            _queue.Enqueue(link);
        }

        if (persist)
        {
            PersistPending(link);
        }
    }

    void PersistPending(DeepLink link)
    {
        if (Options.PersistPendingLinks)
        {
            _store.SavePending(link);
        }
    }

    bool IsDuplicate(DeepLink link)
    {
        if (Options.DeduplicateWindow <= TimeSpan.Zero)
        {
            return false;
        }

        var key = NormalizeUri(link.Uri);
        var now = _clock.UtcNow;
        lock (_gate)
        {
            if (_lastUri is not null
                && string.Equals(_lastUri, key, StringComparison.OrdinalIgnoreCase)
                && now - _lastHandledAt < Options.DeduplicateWindow)
            {
                return true;
            }

            _lastUri = key;
            _lastHandledAt = now;
            return false;
        }
    }

    bool IsExpired(DeepLink? link)
    {
        if (link is null)
        {
            return true;
        }

        if (Options.PendingLinkTtl <= TimeSpan.Zero)
        {
            return false;
        }

        return _clock.UtcNow - link.ReceivedAt > Options.PendingLinkTtl;
    }

    static bool SameUri(DeepLink left, DeepLink right) =>
        string.Equals(NormalizeUri(left.Uri), NormalizeUri(right.Uri), StringComparison.OrdinalIgnoreCase);

    static string NormalizeUri(Uri uri)
    {
        var builder = new UriBuilder(uri) { Fragment = string.Empty };
        return builder.Uri.ToString();
    }

    void FireAndForget(Func<Task> work) => _ = RunSafe(work);

    async Task RunSafe(Func<Task> work)
    {
        try
        {
            await work().ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            Log(DeepLinkLogLevel.Error, "Unhandled dispatch failure.", ex);
        }
    }

    void Log(DeepLinkLogLevel level, string message, Exception? exception = null)
    {
        if (!_logging)
        {
            return;
        }

        (_logger ?? new DebugDeepLinkLogger()).Log(level, message, exception);
    }

    sealed class MappedRoute
    {
        public MappedRoute(
            RoutePattern pattern,
            Func<DeepLinkRoute, CancellationToken, Task>? handler,
            string? shellRoute,
            DeepLinkMapOptions options)
        {
            Pattern = pattern;
            Handler = handler;
            ShellRoute = shellRoute;
            Options = options;
        }

        public RoutePattern Pattern { get; }
        public Func<DeepLinkRoute, CancellationToken, Task>? Handler { get; }
        public string? ShellRoute { get; }
        public DeepLinkMapOptions Options { get; }
    }
}
