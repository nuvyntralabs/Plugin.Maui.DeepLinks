namespace Plugin.Maui.DeepLinks;

/// <summary>
/// Entry point for deep links when dependency injection is not used.
/// </summary>
public static partial class DeepLinks
{
    static IDeepLinks? _current;

    /// <summary>
    /// Gets the shared <see cref="IDeepLinks"/> instance.
    /// </summary>
    public static IDeepLinks Current => _current ??= Create(new DeepLinksOptions());

    /// <summary>
    /// Maps a path template to a handler.
    /// </summary>
    /// <example>
    /// <code>
    /// DeepLinks.Map(
    ///     "/orders/{id}",
    ///     async route =>
    ///     {
    ///         await Shell.Current.GoToAsync($"order?id={route["id"]}");
    ///     });
    /// </code>
    /// </example>
    public static IDeepLinks Map(string template, Func<DeepLinkRoute, Task> handler) =>
        Current.Map(template, handler);

    /// <summary>
    /// Maps a path template to a handler, optionally requiring a signed-in user.
    /// </summary>
    public static IDeepLinks Map(string template, Func<DeepLinkRoute, Task> handler, bool requiresAuthentication) =>
        Current.Map(template, handler, requiresAuthentication);

    /// <summary>
    /// Maps a path template to a handler with per-route options.
    /// </summary>
    public static IDeepLinks Map(string template, Func<DeepLinkRoute, Task> handler, DeepLinkMapOptions options) =>
        Current.Map(template, handler, options);

    /// <summary>
    /// Maps a path template to a Shell route. Tokens such as <c>{id}</c> are replaced from the match.
    /// </summary>
    public static IDeepLinks Map(string template, string shellRoute, bool requiresAuthentication = false) =>
        Current.Map(template, shellRoute, requiresAuthentication);

    /// <summary>
    /// Parses and dispatches a URI string.
    /// </summary>
    public static Task<DeepLinkResult> HandleAsync(string uri, DeepLinkLaunch launch = DeepLinkLaunch.Warm, CancellationToken cancellationToken = default) =>
        Current.HandleAsync(uri, launch, cancellationToken);

    /// <summary>
    /// Parses and dispatches a URI.
    /// </summary>
    public static Task<DeepLinkResult> HandleAsync(Uri uri, DeepLinkLaunch launch = DeepLinkLaunch.Warm, CancellationToken cancellationToken = default) =>
        Current.HandleAsync(uri, launch, cancellationToken);

    /// <summary>
    /// Fire-and-forget dispatch of a URI string.
    /// </summary>
    public static void Handle(string uri, DeepLinkLaunch launch = DeepLinkLaunch.Warm) =>
        Current.Handle(uri, launch);

    /// <summary>
    /// Fire-and-forget dispatch of a URI.
    /// </summary>
    public static void Handle(Uri uri, DeepLinkLaunch launch = DeepLinkLaunch.Warm) =>
        Current.Handle(uri, launch);

    /// <summary>
    /// Marks the app ready and flushes queued cold-start links.
    /// </summary>
    public static void MarkReady() => Current.MarkReady();

    /// <summary>
    /// Call after a successful login to restore the original link.
    /// </summary>
    public static Task<DeepLinkResult?> NotifyAuthenticatedAsync(CancellationToken cancellationToken = default) =>
        Current.NotifyAuthenticatedAsync(cancellationToken);

    /// <summary>
    /// Fire-and-forget restore after login.
    /// </summary>
    public static void NotifyAuthenticated() => Current.NotifyAuthenticated();

    /// <summary>
    /// Restores the Shell location captured before the last deep link.
    /// </summary>
    public static Task RestoreNavigationStackAsync(CancellationToken cancellationToken = default) =>
        Current.RestoreNavigationStackAsync(cancellationToken);

    /// <summary>
    /// Creates a router. Used by <c>UseMauiDeepLinks</c> and tests.
    /// </summary>
    public static IDeepLinks Create(DeepLinksOptions? options = null)
    {
        options ??= new DeepLinksOptions();
        var directory = StoragePath.Resolve(options);
        var store = options.Store ?? new FileDeepLinkStore(directory);
        var navigator = options.Navigator ?? new ShellDeepLinkNavigator();
        var authenticator = options.Authenticator
            ?? (options.IsAuthenticated is null ? null : new DelegateDeepLinkAuthenticator(options.IsAuthenticated));
        return new DeepLinksImplementation(options, store, navigator, authenticator, SystemClock.Instance);
    }

    /// <summary>
    /// Replaces the shared instance. Intended for tests and custom implementations.
    /// </summary>
    public static void SetDefault(IDeepLinks implementation) =>
        _current = implementation ?? throw new ArgumentNullException(nameof(implementation));

    internal static DeepLinksImplementation Create(
        DeepLinksOptions options,
        IDeepLinkStore store,
        IDeepLinkNavigator navigator,
        IDeepLinkAuthenticator? authenticator,
        IClock clock) =>
        new(options, store, navigator, authenticator, clock);
}
