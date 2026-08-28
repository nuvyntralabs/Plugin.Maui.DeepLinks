using System.Diagnostics;

namespace Plugin.Maui.DeepLinks;

/// <summary>
/// Writes diagnostics to <see cref="Debug"/>.
/// </summary>
public sealed class DebugDeepLinkLogger : IDeepLinkLogger
{
    /// <inheritdoc />
    public void Log(DeepLinkLogLevel level, string message, Exception? exception = null)
    {
        if (exception is null)
        {
            Debug.WriteLine($"[DeepLinks] {level}: {message}");
            return;
        }

        Debug.WriteLine($"[DeepLinks] {level}: {message}{Environment.NewLine}{exception}");
    }
}
