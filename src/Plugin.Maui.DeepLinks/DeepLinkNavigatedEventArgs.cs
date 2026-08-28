namespace Plugin.Maui.DeepLinks;

/// <summary>
/// Raised after a mapped handler ran or Shell navigation completed.
/// </summary>
public sealed class DeepLinkNavigatedEventArgs : EventArgs
{
    /// <summary>
    /// Creates the event args.
    /// </summary>
    public DeepLinkNavigatedEventArgs(DeepLinkResult result)
    {
        Result = result ?? throw new ArgumentNullException(nameof(result));
    }

    /// <summary>
    /// Successful dispatch result.
    /// </summary>
    public DeepLinkResult Result { get; }
}
