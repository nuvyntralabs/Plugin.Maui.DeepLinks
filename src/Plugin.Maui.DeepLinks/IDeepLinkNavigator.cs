namespace Plugin.Maui.DeepLinks;

/// <summary>
/// Opens Shell (or host) routes and captures / restores the navigation stack.
/// </summary>
public interface IDeepLinkNavigator
{
    /// <summary>
    /// Whether navigation can run now.
    /// </summary>
    bool CanNavigate { get; }

    /// <summary>
    /// Opens <paramref name="route"/> (a Shell path or login path).
    /// </summary>
    Task NavigateAsync(string route, DeepLinkRoute? match, CancellationToken cancellationToken = default);

    /// <summary>
    /// Snapshots the current Shell location.
    /// </summary>
    Task<NavigationSnapshot?> CaptureStackAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Restores a previously captured location.
    /// </summary>
    Task RestoreStackAsync(NavigationSnapshot snapshot, CancellationToken cancellationToken = default);
}
