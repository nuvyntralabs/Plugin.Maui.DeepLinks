namespace Plugin.Maui.DeepLinks;

/// <summary>
/// Whether the app was already running when the link arrived.
/// </summary>
public enum DeepLinkLaunch
{
    /// <summary>
    /// The process (or activity / scene) was created for this link.
    /// </summary>
    Cold = 0,

    /// <summary>
    /// The app was already in memory (foreground or background).
    /// </summary>
    Warm = 1
}
