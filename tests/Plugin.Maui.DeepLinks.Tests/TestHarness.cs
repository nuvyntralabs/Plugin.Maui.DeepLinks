namespace Plugin.Maui.DeepLinks.Tests;

sealed class FakeClock : IClock
{
    public DateTimeOffset UtcNow { get; set; } = DateTimeOffset.UtcNow;

    public void Advance(TimeSpan duration) => UtcNow += duration;
}

sealed class FakeAuthenticator : IDeepLinkAuthenticator
{
    public bool IsAuthenticated { get; set; }
}

sealed class RecordingNavigator : IDeepLinkNavigator
{
    public bool CanNavigate { get; set; } = true;

    public List<string> Navigations { get; } = [];

    public NavigationSnapshot? Snapshot { get; set; } = new()
    {
        Location = "//home",
        Stack = ["HomePage"],
        CapturedAt = new DateTimeOffset(2026, 8, 28, 12, 0, 0, TimeSpan.Zero)
    };

    public List<NavigationSnapshot> Restored { get; } = [];

    public Task NavigateAsync(string route, DeepLinkRoute? match, CancellationToken cancellationToken = default)
    {
        Navigations.Add(route);
        return Task.CompletedTask;
    }

    public Task<NavigationSnapshot?> CaptureStackAsync(CancellationToken cancellationToken = default) =>
        Task.FromResult(Snapshot);

    public Task RestoreStackAsync(NavigationSnapshot snapshot, CancellationToken cancellationToken = default)
    {
        Restored.Add(snapshot);
        Navigations.Add("restore:" + snapshot.Location);
        return Task.CompletedTask;
    }
}

static class Harness
{
    public static (
        DeepLinksImplementation Router,
        FakeClock Clock,
        FakeAuthenticator Auth,
        RecordingNavigator Navigator,
        MemoryDeepLinkStore Store) Create(Action<DeepLinksOptions>? configure = null)
    {
        var clock = new FakeClock();
        var auth = new FakeAuthenticator();
        var navigator = new RecordingNavigator();
        var store = new MemoryDeepLinkStore();
        var options = new DeepLinksOptions
        {
            StorageDirectory = Directory.CreateTempSubdirectory("maui-deeplinks-").FullName,
            Authenticator = auth,
            Navigator = navigator,
            Store = store,
            QueueUntilReady = false,
            DeduplicateWindow = TimeSpan.Zero,
            LoginPath = "//login"
        };
        configure?.Invoke(options);

        var resolvedStore = options.Store ?? store;
        var resolvedNavigator = options.Navigator ?? navigator;
        var resolvedAuth = options.Authenticator ?? auth;
        var router = DeepLinks.Create(options, resolvedStore, resolvedNavigator, resolvedAuth, clock);
        return (router, clock, resolvedAuth as FakeAuthenticator ?? auth, resolvedNavigator as RecordingNavigator ?? navigator, resolvedStore as MemoryDeepLinkStore ?? store);
    }
}
