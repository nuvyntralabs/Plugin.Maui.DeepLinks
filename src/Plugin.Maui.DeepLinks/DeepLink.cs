namespace Plugin.Maui.DeepLinks;

/// <summary>
/// A parsed incoming deep link.
/// </summary>
public sealed class DeepLink
{
    /// <summary>
    /// Creates a parsed deep link.
    /// </summary>
    public DeepLink(
        Uri uri,
        DeepLinkKind kind,
        DeepLinkLaunch launch,
        string path,
        IReadOnlyDictionary<string, string> query,
        DateTimeOffset receivedAt,
        object? native = null)
    {
        Uri = uri ?? throw new ArgumentNullException(nameof(uri));
        Kind = kind;
        Launch = launch;
        Path = path ?? throw new ArgumentNullException(nameof(path));
        Query = query ?? throw new ArgumentNullException(nameof(query));
        ReceivedAt = receivedAt;
        Native = native;
    }

    /// <summary>
    /// Original URI.
    /// </summary>
    public Uri Uri { get; }

    /// <summary>
    /// App Link, Universal Link, or custom scheme.
    /// </summary>
    public DeepLinkKind Kind { get; }

    /// <summary>
    /// Cold start or warm start.
    /// </summary>
    public DeepLinkLaunch Launch { get; }

    /// <summary>
    /// Normalized path used for matching, for example <c>/orders/123</c>.
    /// </summary>
    public string Path { get; }

    /// <summary>
    /// Query string values (last value wins for repeated keys).
    /// </summary>
    public IReadOnlyDictionary<string, string> Query { get; }

    /// <summary>
    /// When the link was received (UTC).
    /// </summary>
    public DateTimeOffset ReceivedAt { get; }

    /// <summary>
    /// Platform payload when available (Android <c>Intent</c>, iOS <c>NSUserActivity</c> / <c>NSUrl</c>).
    /// </summary>
    public object? Native { get; }

    /// <inheritdoc />
    public override string ToString() => $"{Kind} {Launch} {Uri}";
}
