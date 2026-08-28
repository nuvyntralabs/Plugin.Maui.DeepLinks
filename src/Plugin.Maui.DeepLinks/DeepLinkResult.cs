namespace Plugin.Maui.DeepLinks;

/// <summary>
/// Result of handling a deep link.
/// </summary>
public sealed class DeepLinkResult
{
    DeepLinkResult(
        DeepLinkDisposition disposition,
        DeepLink? link,
        DeepLinkRoute? route,
        string? navigationRoute,
        DeepLinkDeferralReason? deferralReason,
        Exception? exception)
    {
        Disposition = disposition;
        Link = link;
        Route = route;
        NavigationRoute = navigationRoute;
        DeferralReason = deferralReason;
        Exception = exception;
    }

    /// <summary>
    /// What happened.
    /// </summary>
    public DeepLinkDisposition Disposition { get; }

    /// <summary>
    /// Parsed incoming link, when available.
    /// </summary>
    public DeepLink? Link { get; }

    /// <summary>
    /// Matched route, when a template matched.
    /// </summary>
    public DeepLinkRoute? Route { get; }

    /// <summary>
    /// Shell path that was opened, when the map used a Shell template.
    /// </summary>
    public string? NavigationRoute { get; }

    /// <summary>
    /// Why dispatch was deferred, when <see cref="Disposition"/> is
    /// <see cref="DeepLinkDisposition.Deferred"/> or <see cref="DeepLinkDisposition.AwaitingAuth"/>.
    /// </summary>
    public DeepLinkDeferralReason? DeferralReason { get; }

    /// <summary>
    /// Failure, when <see cref="Disposition"/> is <see cref="DeepLinkDisposition.Failed"/>.
    /// </summary>
    public Exception? Exception { get; }

    /// <summary>
    /// Whether a handler ran or Shell navigation completed.
    /// </summary>
    public bool Succeeded => Disposition == DeepLinkDisposition.Navigated;

    internal static DeepLinkResult ForNavigated(DeepLink link, DeepLinkRoute route, string? navigationRoute) =>
        new(DeepLinkDisposition.Navigated, link, route, navigationRoute, null, null);

    internal static DeepLinkResult ForDeferred(DeepLink link, DeepLinkDeferralReason reason, DeepLinkRoute? route = null) =>
        new(reason == DeepLinkDeferralReason.AuthenticationRequired
                ? DeepLinkDisposition.AwaitingAuth
                : DeepLinkDisposition.Deferred,
            link,
            route,
            null,
            reason,
            null);

    internal static DeepLinkResult ForUnmatched(DeepLink link) =>
        new(DeepLinkDisposition.Unmatched, link, null, null, null, null);

    internal static DeepLinkResult ForFailed(DeepLink link, Exception exception, DeepLinkRoute? route = null) =>
        new(DeepLinkDisposition.Failed, link, route, null, null, exception);

    internal static DeepLinkResult ForIgnored(DeepLink? link = null) =>
        new(DeepLinkDisposition.Ignored, link, null, null, null, null);

    /// <inheritdoc />
    public override string ToString() =>
        Route is null ? Disposition.ToString() : $"{Disposition} {Route.Template}";
}
