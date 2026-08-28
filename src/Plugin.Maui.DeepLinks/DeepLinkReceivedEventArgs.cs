namespace Plugin.Maui.DeepLinks;

/// <summary>
/// Raised when a deep link is accepted, before matching or deferral.
/// </summary>
public sealed class DeepLinkReceivedEventArgs : EventArgs
{
    /// <summary>
    /// Creates the event args.
    /// </summary>
    public DeepLinkReceivedEventArgs(DeepLink link)
    {
        Link = link ?? throw new ArgumentNullException(nameof(link));
    }

    /// <summary>
    /// Incoming link.
    /// </summary>
    public DeepLink Link { get; }
}
