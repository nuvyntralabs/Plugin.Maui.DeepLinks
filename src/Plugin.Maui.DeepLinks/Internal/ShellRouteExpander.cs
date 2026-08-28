namespace Plugin.Maui.DeepLinks;

static partial class ShellRouteExpander
{
    public static string Expand(string template, DeepLinkRoute route)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(template);
        ArgumentNullException.ThrowIfNull(route);

        return PlaceholderRegex().Replace(template, match =>
        {
            var key = match.Groups[1].Value;
            return route.TryGet(key, out var value) ? Uri.EscapeDataString(value) : string.Empty;
        });
    }

    [GeneratedRegex(@"\{([^{}]+)\}", RegexOptions.CultureInvariant)]
    private static partial Regex PlaceholderRegex();
}
