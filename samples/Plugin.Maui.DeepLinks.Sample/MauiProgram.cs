using Microsoft.Extensions.Logging;

namespace Plugin.Maui.DeepLinks.Sample;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .UseMauiDeepLinks(options =>
            {
                options.EnableLogging = true;
                options.Hosts.Add("example.com");
                options.CustomSchemes.Add("myapp");
                options.IsAuthenticated = () => Session.IsLoggedIn;
                options.LoginPath = "//login";

                options.Map(
                    "/orders/{id}",
                    async route =>
                    {
                        await Shell.Current.GoToAsync($"//order?id={route["id"]}");
                    });

                options.Map(
                    "/account/{section}",
                    async route =>
                    {
                        await Shell.Current.GoToAsync($"//account?section={route["section"]}");
                    },
                    requiresAuthentication: true);
            });

#if DEBUG
        builder.Logging.AddDebug();
#endif

        return builder.Build();
    }
}
