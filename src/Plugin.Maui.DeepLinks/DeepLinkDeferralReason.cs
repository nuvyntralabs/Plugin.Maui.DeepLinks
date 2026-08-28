namespace Plugin.Maui.DeepLinks;

/// <summary>
/// Why a deep link was not dispatched immediately.
/// </summary>
public enum DeepLinkDeferralReason
{
    /// <summary>
    /// Shell (or the host navigator) is not ready. Flushed by <see cref="IDeepLinks.MarkReady"/>.
    /// </summary>
    NotReady = 0,

    /// <summary>
    /// The matched route requires authentication. Resumed by <see cref="IDeepLinks.NotifyAuthenticatedAsync"/>.
    /// </summary>
    AuthenticationRequired = 1
}
