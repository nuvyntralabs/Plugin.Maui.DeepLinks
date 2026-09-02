namespace Plugin.Maui.DeepLinks.Tests;

public sealed class DeepLinkParserTests
{
    [Fact]
    public void Https_Uses_Absolute_Path()
    {
        var options = new DeepLinksOptions();
        options.Hosts.Add("example.com");

        Assert.True(DeepLinkParser.TryParse(
            "https://example.com/orders/123?utm=email",
            DeepLinkLaunch.Cold,
            options,
            out var link));

        Assert.Equal("/orders/123", link!.Path);
        Assert.Equal("email", link.Query["utm"]);
        Assert.Equal(DeepLinkKind.AppLink, link.Kind);
        Assert.Equal(DeepLinkLaunch.Cold, link.Launch);
    }

    [Fact]
    public void Custom_Scheme_Combines_Host_And_Path()
    {
        var options = new DeepLinksOptions();
        options.CustomSchemes.Add("myapp");

        Assert.True(DeepLinkParser.TryParse(
            "myapp://orders/123",
            DeepLinkLaunch.Warm,
            options,
            out var link));

        Assert.Equal("/orders/123", link!.Path);
        Assert.Equal(DeepLinkKind.CustomScheme, link.Kind);
    }

    [Fact]
    public void Custom_Scheme_Triple_Slash_Uses_Path()
    {
        var options = new DeepLinksOptions();
        options.CustomSchemes.Add("myapp");

        Assert.True(DeepLinkParser.TryParse(
            "myapp:///orders/123",
            DeepLinkLaunch.Warm,
            options,
            out var link));

        Assert.Equal("/orders/123", link!.Path);
    }

    [Fact]
    public void Custom_Scheme_With_Known_Host_Uses_Absolute_Path()
    {
        var options = new DeepLinksOptions();
        options.Hosts.Add("example.com");
        options.CustomSchemes.Add("myapp");

        Assert.True(DeepLinkParser.TryParse(
            "myapp://example.com/orders/123",
            DeepLinkLaunch.Warm,
            options,
            out var link));

        Assert.Equal("/orders/123", link!.Path);
    }

    [Fact]
    public void Rejects_Unknown_Host()
    {
        var options = new DeepLinksOptions();
        options.Hosts.Add("example.com");

        Assert.False(DeepLinkParser.TryParse(
            "https://other.test/orders/123",
            DeepLinkLaunch.Warm,
            options,
            out _));
    }

    [Fact]
    public void Wildcard_Host_Matches_Subdomain()
    {
        var options = new DeepLinksOptions();
        options.Hosts.Add("*.example.com");

        Assert.True(DeepLinkParser.HostMatches("shop.example.com", options.Hosts));
        Assert.True(DeepLinkParser.HostMatches("example.com", options.Hosts));
        Assert.False(DeepLinkParser.HostMatches("evil.com", options.Hosts));
    }

    [Fact]
    public void Rejects_Unknown_Custom_Scheme()
    {
        var options = new DeepLinksOptions();
        options.CustomSchemes.Add("myapp");

        Assert.False(DeepLinkParser.TryParse(
            "other://orders/123",
            DeepLinkLaunch.Warm,
            options,
            out _));
    }

    [Fact]
    public void Query_Last_Value_Wins_And_Decodes()
    {
        var query = DeepLinkParser.ParseQuery(new Uri("https://example.com/search?q=a+b&q=c%26d"));
        Assert.Equal("c&d", query["q"]);
    }

    [Fact]
    public void Empty_Allowlists_Reject_By_Default()
    {
        var options = new DeepLinksOptions();

        Assert.False(DeepLinkParser.TryParse("https://evil.com/admin", DeepLinkLaunch.Warm, options, out _));
        Assert.False(DeepLinkParser.TryParse("myapp://orders/1", DeepLinkLaunch.Warm, options, out _));
        Assert.False(DeepLinkParser.HostMatches("evil.com", options.Hosts));
    }

    [Fact]
    public void PermissiveMode_Accepts_Any_Host_And_Scheme()
    {
        var options = new DeepLinksOptions { PermissiveMode = true };

        Assert.True(DeepLinkParser.TryParse("https://evil.com/admin", DeepLinkLaunch.Warm, options, out var https));
        Assert.Equal("/admin", https!.Path);
        Assert.True(DeepLinkParser.TryParse("other://orders/1", DeepLinkLaunch.Warm, options, out var custom));
        Assert.Equal("/orders/1", custom!.Path);
    }

    [Fact]
    public void Rejects_Cleartext_Http_Unless_Allowed()
    {
        var options = new DeepLinksOptions();
        options.Hosts.Add("example.com");

        Assert.False(DeepLinkParser.TryParse("http://example.com/orders/1", DeepLinkLaunch.Warm, options, out _));

        options.AllowInsecureHttp = true;
        Assert.True(DeepLinkParser.TryParse("http://example.com/orders/1", DeepLinkLaunch.Warm, options, out var link));
        Assert.Equal("/orders/1", link!.Path);
    }
}
