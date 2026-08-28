namespace Plugin.Maui.DeepLinks.Tests;

public sealed class NavigationStackTests
{
    [Fact]
    public async Task Dispatch_Captures_Stack_Before_Handler()
    {
        var (router, _, _, navigator, store) = Harness.Create();
        router.Map("/orders/{id}", _ => Task.CompletedTask);

        await router.HandleAsync("https://example.com/orders/1");

        Assert.True(router.HasSavedNavigationStack);
        Assert.Equal("//home", store.LoadStack()!.Location);
        Assert.Equal(["HomePage"], store.LoadStack()!.Stack);
        Assert.NotNull(navigator.Snapshot);
    }

    [Fact]
    public async Task Restore_Uses_Captured_Location()
    {
        var (router, _, _, navigator, _) = Harness.Create();
        router.Map("/orders/{id}", _ => Task.CompletedTask);
        await router.HandleAsync("https://example.com/orders/1");

        await router.RestoreNavigationStackAsync();

        Assert.Contains("restore://home", navigator.Navigations);
    }

    [Fact]
    public async Task Capture_Can_Be_Disabled_Per_Route()
    {
        var (router, _, _, _, store) = Harness.Create();
        router.Map("/orders/{id}", _ => Task.CompletedTask, new DeepLinkMapOptions
        {
            CaptureNavigationStack = false
        });

        await router.HandleAsync("https://example.com/orders/1");
        Assert.False(router.HasSavedNavigationStack);
        Assert.Null(store.LoadStack());
    }

    [Fact]
    public async Task Auth_Flow_Captures_Stack_Before_Login()
    {
        var (router, _, _, _, store) = Harness.Create();
        router.Map("/account/{section}", _ => Task.CompletedTask, requiresAuthentication: true);

        await router.HandleAsync("https://example.com/account/settings");

        Assert.Equal("//home", store.LoadStack()!.Location);
        Assert.NotNull(store.LoadPending());
    }
}
