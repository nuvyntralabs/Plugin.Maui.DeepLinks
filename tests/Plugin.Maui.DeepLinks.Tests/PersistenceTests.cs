namespace Plugin.Maui.DeepLinks.Tests;

public sealed class PersistenceTests
{
    [Fact]
    public void File_Store_Round_Trips_Pending_Link_And_Stack()
    {
        var root = Directory.CreateTempSubdirectory("maui-deeplinks-file-").FullName;
        var store = new FileDeepLinkStore(root);
        var link = new DeepLink(
            new Uri("https://example.com/orders/55"),
            DeepLinkKind.AppLink,
            DeepLinkLaunch.Cold,
            "/orders/55",
            new Dictionary<string, string> { ["src"] = "push" },
            new DateTimeOffset(2026, 8, 28, 12, 0, 0, TimeSpan.Zero));

        store.SavePending(link);
        store.SaveStack(new NavigationSnapshot
        {
            Location = "//home/catalog",
            Stack = ["HomePage", "CatalogPage"],
            CapturedAt = link.ReceivedAt
        });

        var loaded = store.LoadPending();
        Assert.NotNull(loaded);
        Assert.Equal(link.Uri, loaded!.Uri);
        Assert.Equal("/orders/55", loaded.Path);
        Assert.Equal("push", loaded.Query["src"]);
        Assert.Equal(DeepLinkLaunch.Cold, loaded.Launch);

        var stack = store.LoadStack();
        Assert.Equal("//home/catalog", stack!.Location);
        Assert.Equal(["HomePage", "CatalogPage"], stack.Stack);

        store.ClearPending();
        store.ClearStack();
        Assert.Null(store.LoadPending());
        Assert.Null(store.LoadStack());
    }

    [Fact]
    public async Task Auth_Pending_Survives_New_Router_Instance()
    {
        var store = new MemoryDeepLinkStore();
        var first = Harness.Create(options =>
        {
            options.Store = store;
            options.Authenticator = new FakeAuthenticator { IsAuthenticated = false };
        });

        first.Router.Map("/orders/{id}", _ => Task.CompletedTask, requiresAuthentication: true);
        await first.Router.HandleAsync("https://example.com/orders/77");
        Assert.NotNull(store.LoadPending());

        var auth = new FakeAuthenticator { IsAuthenticated = true };
        var second = Harness.Create(options =>
        {
            options.Store = store;
            options.Authenticator = auth;
        });

        string? id = null;
        second.Router.Map("/orders/{id}", route =>
        {
            id = route["id"];
            return Task.CompletedTask;
        }, requiresAuthentication: true);

        var restored = await second.Router.NotifyAuthenticatedAsync();
        Assert.True(restored!.Succeeded);
        Assert.Equal("77", id);
    }
}
