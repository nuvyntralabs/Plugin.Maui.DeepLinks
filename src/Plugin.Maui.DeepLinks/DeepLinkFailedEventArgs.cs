namespace Plugin.Maui.DeepLinks;

/// <summary>
/// Raised when a handler or navigator throws.
/// </summary>
public sealed class DeepLinkFailedEventArgs : EventArgs
{
    /// <summary>
    /// Creates the event args.
    /// </summary>
    public DeepLinkFailedEventArgs(DeepLink link, Exception exception, DeepLinkRoute? route = null)
    {
        Link = link ?? throw new ArgumentNullException(nameof(link));
        Exception = exception ?? throw new ArgumentNullException(nameof(exception));
        Route = route;
    }

    /// <summary>
    /// Link that failed.
    /// </summary>
    public DeepLink Link { get; }

    /// <summary>
    /// Failure.
    /// </summary>
    public Exception Exception { get; }

    /// <summary>
    /// Matched route, when matching succeeded before the failure.
    /// </summary>
    public DeepLinkRoute? Route { get; }
}
