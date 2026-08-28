namespace Plugin.Maui.DeepLinks;

/// <summary>
/// In-memory store for tests and hosts that do not want disk persistence.
/// </summary>
public sealed class MemoryDeepLinkStore : IDeepLinkStore
{
    readonly object _gate = new();
    DeepLink? _pending;
    NavigationSnapshot? _stack;

    /// <inheritdoc />
    public void SavePending(DeepLink link)
    {
        ArgumentNullException.ThrowIfNull(link);
        lock (_gate)
        {
            _pending = link;
        }
    }

    /// <inheritdoc />
    public DeepLink? LoadPending()
    {
        lock (_gate)
        {
            return _pending;
        }
    }

    /// <inheritdoc />
    public void ClearPending()
    {
        lock (_gate)
        {
            _pending = null;
        }
    }

    /// <inheritdoc />
    public void SaveStack(NavigationSnapshot snapshot)
    {
        ArgumentNullException.ThrowIfNull(snapshot);
        lock (_gate)
        {
            _stack = snapshot;
        }
    }

    /// <inheritdoc />
    public NavigationSnapshot? LoadStack()
    {
        lock (_gate)
        {
            return _stack;
        }
    }

    /// <inheritdoc />
    public void ClearStack()
    {
        lock (_gate)
        {
            _stack = null;
        }
    }
}
