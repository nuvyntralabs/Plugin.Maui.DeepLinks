namespace Plugin.Maui.DeepLinks;

/// <summary>
/// Maps incoming URIs to handlers and Shell screens.
/// </summary>
public interface IDeepLinks
{
    /// <summary>
    /// Always <c>true</c> on Android, iOS, and the shared <c>net10.0</c> surface.
    /// </summary>
    bool IsSupported { get; }

    /// <summary>
    /// Native capabilities for this target.
    /// </summary>
    DeepLinksPlatformInfo Platform { get; }

    /// <summary>
    /// Options the router was created with.
    /// </summary>
    DeepLinksOptions Options { get; }

    /// <summary>
    /// Whether queued cold-start links may be flushed.
    /// </summary>
    bool IsReady { get; }

    /// <summary>
    /// Pending authentication-deferred (or persisted cold-start) link, if any.
    /// </summary>
    DeepLink? Pending { get; }

    /// <summary>
    /// Whether a navigation snapshot is stored.
    /// </summary>
    bool HasSavedNavigationStack { get; }

    /// <summary>
    /// Raised when a candidate URI is accepted.
    /// </summary>
    event EventHandler<DeepLinkReceivedEventArgs>? Received;

    /// <summary>
    /// Raised after a handler or Shell navigation completed.
    /// </summary>
    event EventHandler<DeepLinkNavigatedEventArgs>? Navigated;

    /// <summary>
    /// Raised when a link is held for Shell-ready or login.
    /// </summary>
    event EventHandler<DeepLinkDeferredEventArgs>? Deferred;

    /// <summary>
    /// Raised when no template matches.
    /// </summary>
    event EventHandler<DeepLinkUnhandledEventArgs>? Unhandled;

    /// <summary>
    /// Raised when a handler or navigator throws.
    /// </summary>
    event EventHandler<DeepLinkFailedEventArgs>? Failed;

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
    IDeepLinks Map(string template, Func<DeepLinkRoute, Task> handler);

    /// <summary>
    /// Maps a path template to a handler, optionally requiring a signed-in user.
    /// </summary>
    IDeepLinks Map(string template, Func<DeepLinkRoute, Task> handler, bool requiresAuthentication);

    /// <summary>
    /// Maps a path template to a handler with per-route options.
    /// </summary>
    IDeepLinks Map(string template, Func<DeepLinkRoute, Task> handler, DeepLinkMapOptions options);

    /// <summary>
    /// Maps a path template to a cancellable handler.
    /// </summary>
    IDeepLinks Map(string template, Func<DeepLinkRoute, CancellationToken, Task> handler, DeepLinkMapOptions? options = null);

    /// <summary>
    /// Maps a path template to a synchronous handler.
    /// </summary>
    IDeepLinks Map(string template, Action<DeepLinkRoute> handler, DeepLinkMapOptions? options = null);

    /// <summary>
    /// Maps a path template to a Shell route. Tokens such as <c>{id}</c> are replaced from the match.
    /// </summary>
    IDeepLinks Map(string template, string shellRoute, bool requiresAuthentication = false);

    /// <summary>
    /// Removes the map for <paramref name="template"/>.
    /// </summary>
    bool Unmap(string template);

    /// <summary>
    /// Parses and dispatches a URI string.
    /// </summary>
    Task<DeepLinkResult> HandleAsync(string uri, DeepLinkLaunch launch = DeepLinkLaunch.Warm, CancellationToken cancellationToken = default);

    /// <summary>
    /// Parses and dispatches a URI.
    /// </summary>
    Task<DeepLinkResult> HandleAsync(Uri uri, DeepLinkLaunch launch = DeepLinkLaunch.Warm, CancellationToken cancellationToken = default);

    /// <summary>
    /// Dispatches an already-parsed link.
    /// </summary>
    Task<DeepLinkResult> HandleAsync(DeepLink link, CancellationToken cancellationToken = default);

    /// <summary>
    /// Fire-and-forget dispatch of a URI string.
    /// </summary>
    void Handle(string uri, DeepLinkLaunch launch = DeepLinkLaunch.Warm);

    /// <summary>
    /// Fire-and-forget dispatch of a URI.
    /// </summary>
    void Handle(Uri uri, DeepLinkLaunch launch = DeepLinkLaunch.Warm);

    /// <summary>
    /// Fire-and-forget dispatch of a parsed link.
    /// </summary>
    void Handle(DeepLink link);

    /// <summary>
    /// Marks the app ready (Shell created) and flushes queued cold-start links.
    /// </summary>
    void MarkReady();

    /// <summary>
    /// Call after a successful login. Restores the original authentication-required link.
    /// </summary>
    Task<DeepLinkResult?> NotifyAuthenticatedAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Fire-and-forget <see cref="NotifyAuthenticatedAsync"/>.
    /// </summary>
    void NotifyAuthenticated();

    /// <summary>
    /// Call on logout. Does not clear a pending link (the next login can still restore it).
    /// </summary>
    void NotifyUnauthenticated();

    /// <summary>
    /// Drops the pending link without dispatching it.
    /// </summary>
    bool ClearPending();

    /// <summary>
    /// Restores the Shell location captured before the last deep link.
    /// </summary>
    Task RestoreNavigationStackAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Enables or disables plugin diagnostics.
    /// </summary>
    void EnableLogging(bool enabled, IDeepLinkLogger? logger = null);
}
