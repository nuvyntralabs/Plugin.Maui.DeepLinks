#if ANDROID
using Android.Content;

namespace Plugin.Maui.DeepLinks;

public static partial class DeepLinks
{
    /// <summary>
    /// Forwards an Android VIEW intent (App Link or custom scheme) into the router.
    /// </summary>
    public static void HandleIntent(Intent? intent, DeepLinkLaunch launch = DeepLinkLaunch.Warm) =>
        AndroidDeepLinkBridge.HandleIntent(intent, launch);
}
#endif
