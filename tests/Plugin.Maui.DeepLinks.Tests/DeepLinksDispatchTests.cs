namespace Plugin.Maui.DeepLinks.Tests;

public sealed class DeepLinksDispatchTests
{
    [Fact]
    public async Task Map_Handler_Receives_Route_Indexer()
    {
        var (router, _, _, _, _) = Harness.Create();
        string? id = null;

        router.Map("/orders/{id}", route =>
        {
            id = route["id"];
            return Task.CompletedTask;
        });

        var result = await router.HandleAsync("https://example.com/orders/123");

        Assert.Equal(DeepLinkDisposition.Navigated, result.Disposition);
        Assert.Equal("123", id);
        Assert.Equal("/orders/{id}", result.Route!.Template);
    }

    [Fact]
    public async Task Https_And_Custom_Scheme_Share_The_Same_Map()
    {
        var (router, _, _, _, _) = Harness.Create(options =>
        {
            options.Hosts.Add("example.com");
            options.CustomSchemes.Add("myapp");
        });

        var ids = new List<string>();
        router.Map("/orders/{id}", route =>
        {
            ids.Add(route["id"]);
            return Task.CompletedTask;
        });

        await router.HandleAsync("https://example.com/orders/123");
        await router.HandleAsync("myapp://orders/456");

        Assert.Equal(["123", "456"], ids);
    }

    [Fact]
    public async Task Shell_Route_Expands_Tokens()
    {
        var (router, _, _, navigator, _) = Harness.Create();
        router.Map("/orders/{id}", "order?id={id}");

        var result = await router.HandleAsync("https://example.com/orders/123");

        Assert.True(result.Succeeded);
        Assert.Equal("order?id=123", result.NavigationRoute);
        Assert.Equal(["order?id=123"], navigator.Navigations);
    }

    [Fact]
    public async Task Unmatched_Raises_Event()
    {
        var (router, _, _, _, _) = Harness.Create();
        DeepLink? unmatched = null;
        router.Unhandled += (_, e) => unmatched = e.Link;
        router.Map("/orders/{id}", _ => Task.CompletedTask);

        var result = await router.HandleAsync("https://example.com/unknown");

        Assert.Equal(DeepLinkDisposition.Unmatched, result.Disposition);
        Assert.NotNull(unmatched);
    }

    [Fact]
    public async Task More_Specific_Map_Wins()
    {
        var (router, _, _, _, _) = Harness.Create();
        string? template = null;
        router.Map("/orders/{id}", route =>
        {
            template = route.Template;
            return Task.CompletedTask;
        });
        router.Map("/orders/mine", route =>
        {
            template = route.Template;
            return Task.CompletedTask;
        });

        await router.HandleAsync("https://example.com/orders/mine");
        Assert.Equal("/orders/mine", template);
    }

    [Fact]
    public async Task Query_Is_Available_On_Route()
    {
        var (router, _, _, _, _) = Harness.Create();
        string? source = null;
        router.Map("/orders/{id}", route =>
        {
            source = route["src"];
            return Task.CompletedTask;
        });

        await router.HandleAsync("https://example.com/orders/123?src=email");
        Assert.Equal("email", source);
    }

    [Fact]
    public async Task Handler_Failure_Is_Reported()
    {
        var (router, _, _, _, _) = Harness.Create();
        router.Map("/orders/{id}", _ => throw new InvalidOperationException("boom"));

        var result = await router.HandleAsync("https://example.com/orders/1");

        Assert.Equal(DeepLinkDisposition.Failed, result.Disposition);
        Assert.Equal("boom", result.Exception!.Message);
    }

    [Fact]
    public async Task Duplicate_Within_Window_Is_Ignored()
    {
        var (router, _, _, _, _) = Harness.Create(options =>
        {
            options.DeduplicateWindow = TimeSpan.FromSeconds(2);
        });

        var count = 0;
        router.Map("/orders/{id}", _ =>
        {
            count++;
            return Task.CompletedTask;
        });

        await router.HandleAsync("https://example.com/orders/1");
        var second = await router.HandleAsync("https://example.com/orders/1");

        Assert.Equal(1, count);
        Assert.Equal(DeepLinkDisposition.Ignored, second.Disposition);
    }

    [Fact]
    public async Task Filtered_Host_Is_Ignored()
    {
        var (router, _, _, _, _) = Harness.Create(options => options.Hosts.Add("example.com"));
        router.Map("/orders/{id}", _ => Task.CompletedTask);

        var result = await router.HandleAsync("https://other.test/orders/1");
        Assert.Equal(DeepLinkDisposition.Ignored, result.Disposition);
    }
}
