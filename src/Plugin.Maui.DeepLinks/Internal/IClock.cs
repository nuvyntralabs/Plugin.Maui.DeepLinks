namespace Plugin.Maui.DeepLinks;

interface IClock
{
    DateTimeOffset UtcNow { get; }
}
