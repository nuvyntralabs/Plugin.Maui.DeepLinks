namespace Plugin.Maui.DeepLinks;

/// <summary>
/// Diagnostic sink for the router.
/// </summary>
public interface IDeepLinkLogger
{
    /// <summary>
    /// Writes a diagnostic message.
    /// </summary>
    void Log(DeepLinkLogLevel level, string message, Exception? exception = null);
}

/// <summary>
/// Severity for <see cref="IDeepLinkLogger"/>.
/// </summary>
public enum DeepLinkLogLevel
{
    /// <summary>Verbose tracing.</summary>
    Debug = 0,

    /// <summary>Normal operation.</summary>
    Information = 1,

    /// <summary>Recoverable problem.</summary>
    Warning = 2,

    /// <summary>Handler or navigator failure.</summary>
    Error = 3
}
