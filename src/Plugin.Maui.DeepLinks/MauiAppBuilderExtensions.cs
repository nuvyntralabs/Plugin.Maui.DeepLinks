using Microsoft.Extensions.Logging;
using Microsoft.Maui.Hosting;
using Microsoft.Maui.LifecycleEvents;

namespace Plugin.Maui.DeepLinks;

/// <summary>
/// MAUI host registration for deep links.
/// </summary>
public static class MauiAppBuilderExtensions
{
    /// <summary>
    /// Registers <see cref="IDeepLinks"/> as a singleton and wires Android / iOS link delivery.
    /// </summary>
    /// <example>
    /// <code>
    /// builder.UseMauiDeepLinks(options =>
    /// {
    ///     options.Hosts.Add("example.com");
    ///     options.CustomSchemes.Add("myapp");
    ///     options.IsAuthenticated = () => session.IsLoggedIn;
    ///     options.LoginPath = "//login";
    ///     options.Map("/orders/{id}", async route =>
    ///     {
    ///         await Shell.Current.GoToAsync($"order?id={route["id"]}");
    ///     });
    /// });
    /// </code>
    /// </example>
    public static MauiAppBuilder UseMauiDeepLinks(this MauiAppBuilder builder, Action<DeepLinksOptions>? configure = null)
    {
        ArgumentNullException.ThrowIfNull(builder);

        var options = new DeepLinksOptions();
        configure?.Invoke(options);

        builder.Services.AddMauiDeepLinks(options);
        builder.Services.AddTransient<IMauiInitializeService, DeepLinksInitializer>();

        builder.ConfigureLifecycleEvents(events =>
        {
#if ANDROID
            events.AddAndroid(android =>
            {
                android.OnCreate((activity, _) => AndroidDeepLinkBridge.HandleActivityIntent(activity, DeepLinkLaunch.Cold));
                android.OnNewIntent((activity, intent) =>
                {
                    if (intent is not null)
                    {
                        activity.Intent = intent;
                    }

                    AndroidDeepLinkBridge.HandleIntent(intent, DeepLinkLaunch.Warm);
                });
                android.OnResume(_ => TryMarkReady());
            });
#elif IOS
            events.AddiOS(ios =>
            {
                ios.FinishedLaunching((app, launchOptions) =>
                {
                    IosDeepLinkBridge.HandleLaunchOptions(launchOptions);
                    return false;
                });
                ios.ContinueUserActivity((app, activity, _) =>
                    IosDeepLinkBridge.HandleUserActivity(activity, DeepLinkLaunch.Warm));
                ios.OpenUrl((app, url, _) =>
                    IosDeepLinkBridge.HandleUrl(url, DeepLinkLaunch.Warm));
                ios.OnActivated(_ => TryMarkReady());
            });
#endif
        });

        return builder;
    }

    static void TryMarkReady()
    {
        if (Shell.Current is not null)
        {
            DeepLinks.Current.MarkReady();
        }
    }

    internal static IDeepLinkLogger? CreateLoggerAdapter(IServiceProvider serviceProvider)
    {
        var factory = serviceProvider.GetService<ILoggerFactory>();
        return factory is null ? null : new MicrosoftLoggerAdapter(factory.CreateLogger("Plugin.Maui.DeepLinks"));
    }
}
