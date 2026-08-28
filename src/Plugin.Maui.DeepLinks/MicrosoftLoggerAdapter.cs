using Microsoft.Extensions.Logging;

namespace Plugin.Maui.DeepLinks;

/// <summary>
/// Forwards plugin diagnostics to <see cref="ILogger"/>.
/// </summary>
public sealed class MicrosoftLoggerAdapter : IDeepLinkLogger
{
    readonly ILogger _logger;

    /// <summary>
    /// Creates an adapter around <paramref name="logger"/>.
    /// </summary>
    public MicrosoftLoggerAdapter(ILogger logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc />
    public void Log(DeepLinkLogLevel level, string message, Exception? exception = null)
    {
        var mapped = level switch
        {
            DeepLinkLogLevel.Debug => LogLevel.Debug,
            DeepLinkLogLevel.Information => LogLevel.Information,
            DeepLinkLogLevel.Warning => LogLevel.Warning,
            DeepLinkLogLevel.Error => LogLevel.Error,
            _ => LogLevel.Information
        };

        if (exception is null)
        {
            _logger.Log(mapped, "{Message}", message);
            return;
        }

        _logger.Log(mapped, exception, "{Message}", message);
    }
}
