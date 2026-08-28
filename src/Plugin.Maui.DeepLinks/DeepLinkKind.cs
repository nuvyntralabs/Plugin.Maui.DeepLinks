namespace Plugin.Maui.DeepLinks;

/// <summary>
/// How the incoming URI was delivered.
/// </summary>
public enum DeepLinkKind
{
    /// <summary>
    /// Could not classify the URI.
    /// </summary>
    Unknown = 0,

    /// <summary>
    /// Android App Link (<c>https</c> / <c>http</c> verified intent).
    /// </summary>
    AppLink = 1,

    /// <summary>
    /// iOS Universal Link (<c>https</c> / <c>http</c> associated domain).
    /// </summary>
    UniversalLink = 2,

    /// <summary>
    /// Custom URL scheme such as <c>myapp://orders/123</c>.
    /// </summary>
    CustomScheme = 3
}
