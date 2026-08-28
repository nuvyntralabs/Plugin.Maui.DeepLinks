namespace Plugin.Maui.DeepLinks;

/// <summary>
/// Registers deep-link services without MAUI lifecycle hooks.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds <see cref="IDeepLinks"/> using the supplied options instance.
    /// </summary>
    public static IServiceCollection AddMauiDeepLinks(this IServiceCollection services, DeepLinksOptions options)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(options);

        services.AddSingleton(options);
        services.TryAddSingleton<IDeepLinks>(sp =>
        {
            var resolved = sp.GetService<DeepLinksOptions>() ?? options;
            var router = DeepLinks.Create(resolved);
            DeepLinks.SetDefault(router);
            return router;
        });

        return services;
    }

    /// <summary>
    /// Adds <see cref="IDeepLinks"/> and applies <paramref name="configure"/> to a new options instance.
    /// </summary>
    public static IServiceCollection AddMauiDeepLinks(this IServiceCollection services, Action<DeepLinksOptions>? configure = null)
    {
        ArgumentNullException.ThrowIfNull(services);

        var options = new DeepLinksOptions();
        configure?.Invoke(options);
        return services.AddMauiDeepLinks(options);
    }
}
