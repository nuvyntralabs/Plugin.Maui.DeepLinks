namespace Plugin.Maui.DeepLinks.Tests;

public sealed class RouteMatcherTests
{
    [Theory]
    [InlineData("/orders/{id}", "/orders/123", "id", "123")]
    [InlineData("/orders/{id}/items/{itemId}", "/orders/123/items/9", "itemId", "9")]
    [InlineData("/search", "/search", null, null)]
    public void Matches_Path_Parameters(string template, string path, string? key, string? expected)
    {
        var pattern = RouteMatcher.Compile(template);
        Assert.True(pattern.TryMatch(RouteMatcher.NormalizePath(path), out var values));
        if (key is not null)
        {
            Assert.Equal(expected, values![key]);
        }
    }

    [Fact]
    public void Optional_Segment_Can_Be_Omitted()
    {
        var pattern = RouteMatcher.Compile("/orders/{id?}");
        Assert.True(pattern.TryMatch("/orders", out var empty));
        Assert.False(empty!.ContainsKey("id"));
        Assert.True(pattern.TryMatch("/orders/42", out var values));
        Assert.Equal("42", values!["id"]);
    }

    [Fact]
    public void CatchAll_Captures_Remaining_Segments()
    {
        var pattern = RouteMatcher.Compile("/files/{*path}");
        Assert.True(pattern.TryMatch("/files/a/b/c.txt", out var values));
        Assert.Equal("a/b/c.txt", values!["path"]);
    }

    [Fact]
    public void Int_Constraint_Rejects_Non_Numeric()
    {
        var pattern = RouteMatcher.Compile("/orders/{id:int}");
        Assert.False(pattern.TryMatch("/orders/abc", out _));
        Assert.True(pattern.TryMatch("/orders/42", out var values));
        Assert.Equal("42", values!["id"]);
    }

    [Fact]
    public void Guid_Constraint_Accepts_Guid()
    {
        var id = "aaaaaaaa-bbbb-cccc-dddd-eeeeeeeeeeee";
        var pattern = RouteMatcher.Compile("/users/{id:guid}");
        Assert.True(pattern.TryMatch($"/users/{id}", out var values));
        Assert.Equal(id, values!["id"]);
        Assert.False(pattern.TryMatch("/users/not-a-guid", out _));
    }

    [Fact]
    public void NormalizePath_Strips_Query_And_Trailing_Slash()
    {
        Assert.Equal("/orders/123", RouteMatcher.NormalizePath("/orders/123/?utm=1"));
        Assert.Equal("/", RouteMatcher.NormalizePath("/"));
        Assert.Equal("/orders/{id?}", RouteMatcher.NormalizePath("/orders/{id?}"));
    }

    [Fact]
    public void More_Specific_Template_Wins()
    {
        var exact = RouteMatcher.Compile("/orders/mine");
        var parameterized = RouteMatcher.Compile("/orders/{id}");
        var catchAll = RouteMatcher.Compile("/orders/{*rest}");

        Assert.True(RouteMatcher.CompareSpecificity(exact, parameterized) < 0);
        Assert.True(RouteMatcher.CompareSpecificity(parameterized, catchAll) < 0);
    }

    [Fact]
    public void Case_Insensitive_By_Default()
    {
        var pattern = RouteMatcher.Compile("/Orders/{id}");
        Assert.True(pattern.TryMatch("/orders/123", out var values));
        Assert.Equal("123", values!["id"]);
    }

    [Fact]
    public void Decodes_Path_Parameter()
    {
        var pattern = RouteMatcher.Compile("/search/{q}");
        Assert.True(pattern.TryMatch("/search/red%20shoes", out var values));
        Assert.Equal("red shoes", values!["q"]);
    }
}
