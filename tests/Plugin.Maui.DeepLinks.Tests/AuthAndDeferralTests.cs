namespace Plugin.Maui.DeepLinks.Tests;

public sealed class AuthAndDeferralTests
{
    [Fact]
    public async Task Auth_Required_Link_Waits_For_Login_Then_Restores()
    {
        var (router, _, auth, navigator, store) = Harness.Create();
        string? opened = null;

        router.Map("/orders/{id}", route =>
        {
            opened = route["id"];
            return Task.CompletedTask;
        }, requiresAuthentication: true);

        var first = await router.HandleAsync("https://example.com/orders/123");

        Assert.Equal(DeepLinkDisposition.AwaitingAuth, first.Disposition);
        Assert.Equal(["//login"], navigator.Navigations);
        Assert.NotNull(store.LoadPending());
        Assert.Null(opened);

        auth.IsAuthenticated = true;
        var restored = await router.NotifyAuthenticatedAsync();

        Assert.NotNull(restored);
        Assert.Equal(DeepLinkDisposition.Navigated, restored!.Disposition);
        Assert.Equal("123", opened);
        Assert.Null(store.LoadPending());
    }

    [Fact]
    public async Task Public_Link_Does_Not_Require_Auth()
    {
        var (router, _, auth, navigator, _) = Harness.Create();
        auth.IsAuthenticated = false;
        var opened = false;
        router.Map("/orders/{id}", _ =>
        {
            opened = true;
            return Task.CompletedTask;
        });

        var result = await router.HandleAsync("https://example.com/orders/1");

        Assert.True(result.Succeeded);
        Assert.True(opened);
        Assert.Empty(navigator.Navigations);
    }

    [Fact]
    public async Task Cold_Start_Is_Queued_Until_Ready()
    {
        var (router, _, _, _, _) = Harness.Create(options => options.QueueUntilReady = true);
        var opened = false;
        router.Map("/orders/{id}", _ =>
        {
            opened = true;
            return Task.CompletedTask;
        });

        var queued = await router.HandleAsync("https://example.com/orders/9", DeepLinkLaunch.Cold);
        Assert.Equal(DeepLinkDisposition.Deferred, queued.Disposition);
        Assert.Equal(DeepLinkDeferralReason.NotReady, queued.DeferralReason);
        Assert.False(opened);

        var navigated = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
        router.Navigated += (_, _) => navigated.TrySetResult(true);
        router.MarkReady();

        await navigated.Task.WaitAsync(TimeSpan.FromSeconds(2));
        Assert.True(opened);
    }

    [Fact]
    public async Task NotifyAuthenticated_With_Nothing_Pending_Returns_Null()
    {
        var (router, _, auth, _, _) = Harness.Create();
        auth.IsAuthenticated = true;
        Assert.Null(await router.NotifyAuthenticatedAsync());
    }

    [Fact]
    public async Task Expired_Pending_Link_Is_Dropped()
    {
        var (router, clock, auth, _, store) = Harness.Create();
        router.Map("/orders/{id}", _ => Task.CompletedTask, requiresAuthentication: true);

        await router.HandleAsync("https://example.com/orders/1");
        Assert.NotNull(store.LoadPending());

        clock.Advance(TimeSpan.FromHours(25));
        auth.IsAuthenticated = true;
        var result = await router.NotifyAuthenticatedAsync();

        Assert.Equal(DeepLinkDisposition.Ignored, result!.Disposition);
        Assert.Null(store.LoadPending());
    }

    [Fact]
    public async Task Default_Auth_Requirement_Gates_Every_Map()
    {
        var (router, _, auth, navigator, _) = Harness.Create(options =>
        {
            options.RequireAuthenticationByDefault = true;
        });
        router.Map("/orders/{id}", _ => Task.CompletedTask);

        var result = await router.HandleAsync("https://example.com/orders/1");
        Assert.Equal(DeepLinkDisposition.AwaitingAuth, result.Disposition);
        Assert.Equal("//login", navigator.Navigations.Single());

        auth.IsAuthenticated = true;
        var restored = await router.NotifyAuthenticatedAsync();
        Assert.True(restored!.Succeeded);
    }
}
