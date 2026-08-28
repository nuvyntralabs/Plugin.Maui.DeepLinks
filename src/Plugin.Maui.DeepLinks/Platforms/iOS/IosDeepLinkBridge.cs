#if IOS
using Foundation;
using UIKit;

namespace Plugin.Maui.DeepLinks;

static class IosDeepLinkBridge
{
    public static void HandleLaunchOptions(NSDictionary? launchOptions)
    {
        if (launchOptions is null)
        {
            return;
        }

        if (launchOptions.ContainsKey(UIApplication.LaunchOptionsUrlKey)
            && launchOptions.ObjectForKey(UIApplication.LaunchOptionsUrlKey) is NSUrl launchUrl)
        {
            HandleUrl(launchUrl, DeepLinkLaunch.Cold);
        }

        if (launchOptions.ContainsKey(UIApplication.LaunchOptionsUserActivityDictionaryKey)
            && launchOptions.ObjectForKey(UIApplication.LaunchOptionsUserActivityDictionaryKey) is NSDictionary activities)
        {
            foreach (var key in activities.Keys)
            {
                if (activities.ObjectForKey(key) is NSUserActivity activity)
                {
                    HandleUserActivity(activity, DeepLinkLaunch.Cold);
                }
            }
        }
    }

    public static bool HandleUserActivity(NSUserActivity? activity, DeepLinkLaunch launch)
    {
        if (activity is null)
        {
            return false;
        }

        if (activity.ActivityType != NSUserActivityType.BrowsingWeb)
        {
            return false;
        }

        var url = activity.WebPageUrl;
        if (url is null)
        {
            return false;
        }

        return HandleUrl(url, launch, activity);
    }

    public static bool HandleUrl(NSUrl? url, DeepLinkLaunch launch, object? native = null)
    {
        if (url is null)
        {
            return false;
        }

        var value = url.AbsoluteString;
        if (string.IsNullOrWhiteSpace(value))
        {
            return false;
        }

        if (!DeepLinkParser.TryParse(value, launch, DeepLinks.Current.Options, out var parsed))
        {
            return false;
        }

        var link = new DeepLink(
            parsed.Uri,
            parsed.Kind,
            launch,
            parsed.Path,
            parsed.Query,
            parsed.ReceivedAt,
            native: native ?? url);

        DeepLinks.Current.Handle(link);
        return true;
    }
}
#endif
