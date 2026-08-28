#if ANDROID
using Android.App;
using Android.Content;

namespace Plugin.Maui.DeepLinks;

static class AndroidDeepLinkBridge
{
    public static void HandleActivityIntent(Activity? activity, DeepLinkLaunch launch)
    {
        if (activity?.Intent is null)
        {
            return;
        }

        HandleIntent(activity.Intent, launch);
    }

    public static void HandleIntent(Intent? intent, DeepLinkLaunch launch)
    {
        if (intent?.Data is null)
        {
            return;
        }

        if (intent.Action is not null
            && !string.Equals(intent.Action, Intent.ActionView, StringComparison.Ordinal))
        {
            return;
        }

        var value = intent.Data.ToString();
        if (string.IsNullOrWhiteSpace(value))
        {
            return;
        }

        if (!DeepLinkParser.TryParse(value, launch, DeepLinks.Current.Options, out var parsed))
        {
            return;
        }

        var link = new DeepLink(
            parsed.Uri,
            parsed.Kind,
            launch,
            parsed.Path,
            parsed.Query,
            parsed.ReceivedAt,
            native: intent);

        DeepLinks.Current.Handle(link);
    }
}
#endif
