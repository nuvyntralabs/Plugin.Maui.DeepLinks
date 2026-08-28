using System.Text.Json.Serialization.Metadata;

namespace Plugin.Maui.DeepLinks;

/// <summary>
/// JSON file store for a pending link and a navigation snapshot.
/// </summary>
public sealed class FileDeepLinkStore : IDeepLinkStore
{
    const string PendingFileName = "pending.json";
    const string StackFileName = "stack.json";
    readonly object _gate = new();
    readonly string _directory;

    /// <summary>
    /// Creates a store under <paramref name="directory"/>.
    /// </summary>
    public FileDeepLinkStore(string directory)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(directory);
        _directory = directory;
    }

    /// <inheritdoc />
    public void SavePending(DeepLink link)
    {
        ArgumentNullException.ThrowIfNull(link);
        Write(PendingFileName, PendingLinkRecord.From(link), DeepLinksJsonContext.Default.PendingLinkRecord);
    }

    /// <inheritdoc />
    public DeepLink? LoadPending()
    {
        var record = Read(PendingFileName, DeepLinksJsonContext.Default.PendingLinkRecord);
        return record?.ToDeepLink();
    }

    /// <inheritdoc />
    public void ClearPending() => Delete(PendingFileName);

    /// <inheritdoc />
    public void SaveStack(NavigationSnapshot snapshot)
    {
        ArgumentNullException.ThrowIfNull(snapshot);
        Write(StackFileName, snapshot, DeepLinksJsonContext.Default.NavigationSnapshot);
    }

    /// <inheritdoc />
    public NavigationSnapshot? LoadStack() =>
        Read(StackFileName, DeepLinksJsonContext.Default.NavigationSnapshot);

    /// <inheritdoc />
    public void ClearStack() => Delete(StackFileName);

    void Write<T>(string fileName, T value, JsonTypeInfo<T> typeInfo)
    {
        lock (_gate)
        {
            Directory.CreateDirectory(_directory);
            var path = Path.Combine(_directory, fileName);
            var temp = path + ".tmp";
            var json = JsonSerializer.Serialize(value, typeInfo);
            File.WriteAllText(temp, json);
            File.Copy(temp, path, overwrite: true);
            File.Delete(temp);
        }
    }

    T? Read<T>(string fileName, JsonTypeInfo<T> typeInfo)
    {
        lock (_gate)
        {
            var path = Path.Combine(_directory, fileName);
            if (!File.Exists(path))
            {
                return default;
            }

            try
            {
                var json = File.ReadAllText(path);
                return JsonSerializer.Deserialize(json, typeInfo);
            }
            catch
            {
                return default;
            }
        }
    }

    void Delete(string fileName)
    {
        lock (_gate)
        {
            var path = Path.Combine(_directory, fileName);
            if (File.Exists(path))
            {
                File.Delete(path);
            }
        }
    }
}
