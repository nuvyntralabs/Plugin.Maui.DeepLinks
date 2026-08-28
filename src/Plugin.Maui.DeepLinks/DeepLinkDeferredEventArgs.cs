namespace Plugin.Maui.DeepLinks;

/// <summary>
/// Raised when a link is held for later (Shell not ready, or login required).
/// </summary>
public sealed class DeepLinkDeferredEventArgs : EventArgs
{
    /// <summary>
    /// Creates the event args.
    /// </summary>
    public DeepLinkDeferredEventArgs(DeepLink link, DeepLinkDeferralReason reason, DeepLinkRoute? route = null)
    {
        Link = link ?? throw new ArgumentNullException(nameof(link));
        Reason = reason;
        Route = route;
    }

    /// <summary>
    /// Held link.
    /// </summary>
    public DeepLink Link { get; }

    /// <summary>
    /// Why it was held.
    /// </summary>
    public DeepLinkDeferralReason Reason { get; }

    /// <summary>
    /// Matched route when deferral is for authentication.
    /// </summary>
    public DeepLinkRoute? Route { get; }
}
