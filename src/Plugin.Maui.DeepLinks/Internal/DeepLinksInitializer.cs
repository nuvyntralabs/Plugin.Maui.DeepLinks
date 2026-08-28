using Microsoft.Maui.Hosting;

namespace Plugin.Maui.DeepLinks;

sealed class DeepLinksInitializer : IMauiInitializeService
{
    public void Initialize(IServiceProvider services)
    {
        var options = services.GetService<DeepLinksOptions>() ?? new DeepLinksOptions();
        var router = services.GetService<IDeepLinks>() ?? DeepLinks.Current;
        DeepLinks.SetDefault(router);

        if (options.EnableLogging)
        {
            var logger = options.Logger
                ?? MauiAppBuilderExtensions.CreateLoggerAdapter(services)
                ?? new DebugDeepLinkLogger();
            router.EnableLogging(true, logger);
        }

        MainThread.BeginInvokeOnMainThread(() =>
        {
            if (Shell.Current is not null)
            {
                router.MarkReady();
            }
        });
    }
}
