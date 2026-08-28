namespace Plugin.Maui.DeepLinks;

/// <summary>
/// Configuration for <see cref="DeepLinks"/>.
/// </summary>
public sealed class DeepLinksOptions
{
    /// <summary>
    /// HTTPS hosts accepted for App Links / Universal Links, for example <c>example.com</c>.
    /// Empty means any host. Prefix with <c>*.</c> to allow a domain and its subdomains.
    /// </summary>
    public Collection<string> Hosts { get; } = [];

    /// <summary>
    /// Custom URL schemes, for example <c>myapp</c>. Empty means any non-http(s) scheme is accepted.
    /// </summary>
    public Collection<string> CustomSchemes { get; } = [];

    /// <summary>
    /// Maps registered before the host is built.
    /// </summary>
    public Collection<DeepLinkMapRegistration> Maps { get; } = [];

    /// <summary>
    /// Returns whether the user is signed in. When <c>null</c>, authentication-required routes always run.
    /// </summary>
    public Func<bool>? IsAuthenticated { get; set; }

    /// <summary>
    /// Optional authenticator. Wins over <see cref="IsAuthenticated"/> when set.
    /// </summary>
    public IDeepLinkAuthenticator? Authenticator { get; set; }

    /// <summary>
    /// Shell path opened when an authentication-required link arrives and the user is signed out.
    /// Typical value: <c>//login</c>.
    /// </summary>
    public string? LoginPath { get; set; }

    /// <summary>
    /// Queue incoming links until <see cref="IDeepLinks.MarkReady"/>. Default is <c>true</c>.
    /// </summary>
    public bool QueueUntilReady { get; set; } = true;

    /// <summary>
    /// Persist authentication-deferred (and optionally not-ready) links so they survive process death.
    /// Default is <c>true</c>.
    /// </summary>
    public bool PersistPendingLinks { get; set; } = true;

    /// <summary>
    /// Persist not-ready links as well as authentication-deferred links. Default is <c>true</c>.
    /// </summary>
    public bool PersistNotReadyLinks { get; set; } = true;

    /// <summary>
    /// Capture the current Shell location before dispatching a matched link. Default is <c>true</c>.
    /// </summary>
    public bool CaptureNavigationStack { get; set; } = true;

    /// <summary>
    /// How long a persisted pending link remains valid. Default is 24 hours.
    /// </summary>
    public TimeSpan PendingLinkTtl { get; set; } = TimeSpan.FromHours(24);

    /// <summary>
    /// Ignore the same URI if it arrives again within this window (OnCreate + OnNewIntent).
    /// Default is 1.5 seconds. Set to <see cref="TimeSpan.Zero"/> to disable.
    /// </summary>
    public TimeSpan DeduplicateWindow { get; set; } = TimeSpan.FromMilliseconds(1500);

    /// <summary>
    /// Treat paths as case-insensitive. Default is <c>true</c>.
    /// </summary>
    public bool CaseInsensitivePaths { get; set; } = true;

    /// <summary>
    /// Require authentication for every map unless the map sets
    /// <see cref="DeepLinkMapOptions.RequiresAuthentication"/> to <c>false</c> after you opt in per-route.
    /// Default is <c>false</c> — only maps marked <c>requiresAuthentication: true</c> are gated.
    /// </summary>
    public bool RequireAuthenticationByDefault { get; set; }

    /// <summary>
    /// Write diagnostic messages. Default is <c>false</c>.
    /// </summary>
    public bool EnableLogging { get; set; }

    /// <summary>
    /// Optional logger. When logging is on and this is <c>null</c>, a debug logger is used.
    /// </summary>
    public IDeepLinkLogger? Logger { get; set; }

    /// <summary>
    /// Custom navigator. Defaults to <see cref="ShellDeepLinkNavigator"/>.
    /// </summary>
    public IDeepLinkNavigator? Navigator { get; set; }

    /// <summary>
    /// Custom pending-link / stack store. Defaults to a file store under app data.
    /// </summary>
    public IDeepLinkStore? Store { get; set; }

    /// <summary>
    /// Directory for the default file store. When empty, app data is used.
    /// </summary>
    public string? StorageDirectory { get; set; }

    /// <summary>
    /// Maps <paramref name="template"/> to <paramref name="handler"/>.
    /// </summary>
    public DeepLinksOptions Map(string template, Func<DeepLinkRoute, Task> handler, bool requiresAuthentication = false)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(template);
        ArgumentNullException.ThrowIfNull(handler);
        Maps.Add(new DeepLinkMapRegistration(
            template,
            (route, _) => handler(route),
            new DeepLinkMapOptions { RequiresAuthentication = requiresAuthentication }));
        return this;
    }

    /// <summary>
    /// Maps <paramref name="template"/> to a Shell path. Tokens such as <c>{id}</c> are replaced from the match.
    /// </summary>
    public DeepLinksOptions Map(string template, string shellRoute, bool requiresAuthentication = false)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(template);
        ArgumentException.ThrowIfNullOrWhiteSpace(shellRoute);
        Maps.Add(new DeepLinkMapRegistration(
            template,
            shellRoute,
            new DeepLinkMapOptions { RequiresAuthentication = requiresAuthentication }));
        return this;
    }
}
