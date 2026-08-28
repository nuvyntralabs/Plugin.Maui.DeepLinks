namespace Plugin.Maui.DeepLinks;

/// <summary>
/// Per-route options for <see cref="IDeepLinks.Map(string, Func{DeepLinkRoute, Task}, DeepLinkMapOptions)"/>.
/// </summary>
public sealed class DeepLinkMapOptions
{
    /// <summary>
    /// When <c>true</c>, the link is persisted and login runs first if the user is signed out.
    /// </summary>
    public bool RequiresAuthentication { get; set; }

    /// <summary>
    /// When <c>true</c>, the current Shell location is captured before the handler runs.
    /// Defaults to the global <see cref="DeepLinksOptions.CaptureNavigationStack"/> value when left <c>null</c>.
    /// </summary>
    public bool? CaptureNavigationStack { get; set; }

    /// <summary>
    /// Optional name for diagnostics.
    /// </summary>
    public string? Name { get; set; }
}
