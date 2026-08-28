namespace Plugin.Maui.DeepLinks;

/// <summary>
/// Raised when no template matches the incoming path.
/// </summary>
public sealed class DeepLinkUnhandledEventArgs : EventArgs
{
    /// <summary>
    /// Creates the event args.
    /// </summary>
    public DeepLinkUnhandledEventArgs(DeepLink link)
    {
        Link = link ?? throw new ArgumentNullException(nameof(link));
    }

    /// <summary>
    /// Unmatched link.
    /// </summary>
    public DeepLink Link { get; }
}
