#if ANDROID || IOS
using Microsoft.Maui.Storage;
#endif

namespace Plugin.Maui.DeepLinks;

static class StoragePath
{
    public const string FolderName = "Plugin.Maui.DeepLinks";

    public static string Resolve(DeepLinksOptions options)
    {
        if (!string.IsNullOrWhiteSpace(options.StorageDirectory))
        {
            return options.StorageDirectory;
        }

        var root = TryAppData() ?? Path.Combine(Path.GetTempPath(), FolderName);
        return Path.Combine(root, FolderName);
    }

    static string? TryAppData()
    {
#if ANDROID || IOS
        try
        {
            return FileSystem.AppDataDirectory;
        }
        catch
        {
            return null;
        }
#else
        return null;
#endif
    }
}
