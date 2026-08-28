namespace Plugin.Maui.DeepLinks;

/// <summary>
/// Platform capabilities reported by the plugin.
/// </summary>
public sealed class DeepLinksPlatformInfo
{
    DeepLinksPlatformInfo(string name, bool isSupported, bool appLinks, bool universalLinks, bool customSchemes)
    {
        Name = name;
        IsSupported = isSupported;
        AppLinks = appLinks;
        UniversalLinks = universalLinks;
        CustomSchemes = customSchemes;
    }

    /// <summary>
    /// Platform name.
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// Whether native link delivery is wired on this target.
    /// </summary>
    public bool IsSupported { get; }

    /// <summary>
    /// Android App Links.
    /// </summary>
    public bool AppLinks { get; }

    /// <summary>
    /// iOS Universal Links.
    /// </summary>
    public bool UniversalLinks { get; }

    /// <summary>
    /// Custom URL schemes.
    /// </summary>
    public bool CustomSchemes { get; }

    /// <summary>
    /// Current platform.
    /// </summary>
    public static DeepLinksPlatformInfo Current { get; } =
#if ANDROID
        new("Android", true, appLinks: true, universalLinks: false, customSchemes: true);
#elif IOS
        new("iOS", true, appLinks: false, universalLinks: true, customSchemes: true);
#else
        new("Shared", true, appLinks: false, universalLinks: false, customSchemes: true);
#endif
}
