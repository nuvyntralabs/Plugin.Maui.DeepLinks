#if IOS
using Foundation;

namespace Plugin.Maui.DeepLinks;

public static partial class DeepLinks
{
    /// <summary>
    /// Forwards an iOS URL (custom scheme or Universal Link) into the router.
    /// </summary>
    public static void HandleUrl(NSUrl? url, DeepLinkLaunch launch = DeepLinkLaunch.Warm) =>
        IosDeepLinkBridge.HandleUrl(url, launch);

    /// <summary>
    /// Forwards an iOS user activity (Universal Link) into the router.
    /// </summary>
    public static bool HandleUserActivity(NSUserActivity? activity, DeepLinkLaunch launch = DeepLinkLaunch.Warm) =>
        IosDeepLinkBridge.HandleUserActivity(activity, launch);
}
#endif
