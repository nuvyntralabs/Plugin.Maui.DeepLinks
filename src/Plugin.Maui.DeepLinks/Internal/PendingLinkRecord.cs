namespace Plugin.Maui.DeepLinks;

sealed class PendingLinkRecord
{
    public string? Uri { get; set; }

    public DeepLinkKind Kind { get; set; }

    public DeepLinkLaunch Launch { get; set; }

    public string? Path { get; set; }

    public Dictionary<string, string> Query { get; set; } = new(StringComparer.OrdinalIgnoreCase);

    public DateTimeOffset ReceivedAt { get; set; }

    public static PendingLinkRecord From(DeepLink link) => new()
    {
        Uri = link.Uri.ToString(),
        Kind = link.Kind,
        Launch = link.Launch,
        Path = link.Path,
        Query = new Dictionary<string, string>(link.Query, StringComparer.OrdinalIgnoreCase),
        ReceivedAt = link.ReceivedAt
    };

    public DeepLink? ToDeepLink()
    {
        if (!System.Uri.TryCreate(Uri, UriKind.Absolute, out var uri) || string.IsNullOrWhiteSpace(Path))
        {
            return null;
        }

        return new DeepLink(
            uri,
            Kind,
            Launch,
            Path,
            Query,
            ReceivedAt);
    }
}
