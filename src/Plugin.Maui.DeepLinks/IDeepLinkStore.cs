namespace Plugin.Maui.DeepLinks;

/// <summary>
/// Persists a pending deep link and a navigation snapshot across process death.
/// </summary>
public interface IDeepLinkStore
{
    /// <summary>
    /// Saves the link that should resume after login or after a cold start.
    /// </summary>
    void SavePending(DeepLink link);

    /// <summary>
    /// Loads the pending link, or <c>null</c>.
    /// </summary>
    DeepLink? LoadPending();

    /// <summary>
    /// Clears the pending link.
    /// </summary>
    void ClearPending();

    /// <summary>
    /// Saves a navigation snapshot.
    /// </summary>
    void SaveStack(NavigationSnapshot snapshot);

    /// <summary>
    /// Loads the last snapshot, or <c>null</c>.
    /// </summary>
    NavigationSnapshot? LoadStack();

    /// <summary>
    /// Clears the saved snapshot.
    /// </summary>
    void ClearStack();
}
