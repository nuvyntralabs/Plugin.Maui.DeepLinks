namespace Plugin.Maui.DeepLinks;

/// <summary>
/// A captured Shell navigation location that can be restored after a deep link.
/// </summary>
public sealed class NavigationSnapshot
{
    /// <summary>
    /// Shell location, for example <c>//home/catalog</c>.
    /// </summary>
    public string? Location { get; set; }

    /// <summary>
    /// Individual stack entries when the navigator could enumerate them.
    /// </summary>
    public List<string> Stack { get; set; } = [];

    /// <summary>
    /// When the snapshot was taken (UTC).
    /// </summary>
    public DateTimeOffset CapturedAt { get; set; }

    /// <summary>
    /// Whether there is anything to restore.
    /// </summary>
    public bool HasLocation => !string.IsNullOrWhiteSpace(Location);
}
