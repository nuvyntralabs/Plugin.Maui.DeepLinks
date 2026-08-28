namespace Plugin.Maui.DeepLinks;

[JsonSourceGenerationOptions(
    PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase,
    WriteIndented = false,
    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull)]
[JsonSerializable(typeof(PendingLinkRecord))]
[JsonSerializable(typeof(NavigationSnapshot))]
[JsonSerializable(typeof(List<string>))]
sealed partial class DeepLinksJsonContext : JsonSerializerContext;
