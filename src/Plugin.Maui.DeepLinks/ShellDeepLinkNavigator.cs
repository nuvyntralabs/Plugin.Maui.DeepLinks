namespace Plugin.Maui.DeepLinks;

/// <summary>
/// Default navigator that uses <see cref="Shell.Current"/>.
/// </summary>
public sealed class ShellDeepLinkNavigator : IDeepLinkNavigator
{
    /// <inheritdoc />
    public bool CanNavigate
    {
        get
        {
            try
            {
                return Shell.Current is not null;
            }
            catch
            {
                return false;
            }
        }
    }

    /// <inheritdoc />
    public Task NavigateAsync(string route, DeepLinkRoute? match, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(route);
        return InvokeOnMainThreadAsync(() =>
        {
            var shell = Shell.Current ?? throw new InvalidOperationException(
                "Shell.Current is not available. Call DeepLinks.MarkReady() after AppShell is created.");
            return shell.GoToAsync(route);
        });
    }

    /// <inheritdoc />
    public Task<NavigationSnapshot?> CaptureStackAsync(CancellationToken cancellationToken = default)
    {
        return InvokeOnMainThreadAsync(() =>
        {
            var shell = Shell.Current;
            if (shell is null)
            {
                return Task.FromResult<NavigationSnapshot?>(null);
            }

            var location = shell.CurrentState?.Location?.ToString();
            var stack = new List<string>();
            if (shell.Navigation?.NavigationStack is { } pages)
            {
                foreach (var page in pages)
                {
                    if (page is null)
                    {
                        continue;
                    }

                    stack.Add(page.GetType().Name);
                }
            }

            if (string.IsNullOrWhiteSpace(location) && stack.Count == 0)
            {
                return Task.FromResult<NavigationSnapshot?>(null);
            }

            return Task.FromResult<NavigationSnapshot?>(new NavigationSnapshot
            {
                Location = location,
                Stack = stack,
                CapturedAt = DateTimeOffset.UtcNow
            });
        });
    }

    /// <inheritdoc />
    public Task RestoreStackAsync(NavigationSnapshot snapshot, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(snapshot);
        if (string.IsNullOrWhiteSpace(snapshot.Location))
        {
            return Task.CompletedTask;
        }

        return InvokeOnMainThreadAsync(() =>
        {
            var shell = Shell.Current ?? throw new InvalidOperationException(
                "Shell.Current is not available. Call DeepLinks.MarkReady() after AppShell is created.");
            return shell.GoToAsync(snapshot.Location);
        });
    }

    static Task InvokeOnMainThreadAsync(Func<Task> work)
    {
#if ANDROID || IOS
        try
        {
            if (!MainThread.IsMainThread)
            {
                return MainThread.InvokeOnMainThreadAsync(work);
            }
        }
        catch (InvalidOperationException)
        {
            // No UI synchronization context (tests).
        }
#endif
        return work();
    }

    static Task<T> InvokeOnMainThreadAsync<T>(Func<Task<T>> work)
    {
#if ANDROID || IOS
        try
        {
            if (!MainThread.IsMainThread)
            {
                return MainThread.InvokeOnMainThreadAsync(work);
            }
        }
        catch (InvalidOperationException)
        {
            // No UI synchronization context (tests).
        }
#endif
        return work();
    }
}
