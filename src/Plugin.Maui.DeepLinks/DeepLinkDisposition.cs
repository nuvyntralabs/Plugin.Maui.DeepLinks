namespace Plugin.Maui.DeepLinks;

/// <summary>
/// Outcome of attempting to handle a deep link.
/// </summary>
public enum DeepLinkDisposition
{
    /// <summary>
    /// A mapped handler ran and/or Shell navigation completed.
    /// </summary>
    Navigated = 0,

    /// <summary>
    /// Held until <see cref="IDeepLinks.MarkReady"/> because Shell is not ready.
    /// </summary>
    Deferred = 1,

    /// <summary>
    /// Held until <see cref="IDeepLinks.NotifyAuthenticatedAsync"/> because the route requires a signed-in user.
    /// </summary>
    AwaitingAuth = 2,

    /// <summary>
    /// No registered template matched the path.
    /// </summary>
    Unmatched = 3,

    /// <summary>
    /// A handler or navigator threw.
    /// </summary>
    Failed = 4,

    /// <summary>
    /// Dropped (duplicate, filtered host/scheme, or expired pending link).
    /// </summary>
    Ignored = 5
}
