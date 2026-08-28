namespace Plugin.Maui.DeepLinks;

/// <summary>
/// A route map registered on <see cref="DeepLinksOptions"/> before the host is built.
/// </summary>
public sealed class DeepLinkMapRegistration
{
    /// <summary>
    /// Creates a map that invokes a handler.
    /// </summary>
    public DeepLinkMapRegistration(string template, Func<DeepLinkRoute, CancellationToken, Task> handler, DeepLinkMapOptions? options = null)
    {
        Template = template ?? throw new ArgumentNullException(nameof(template));
        Handler = handler ?? throw new ArgumentNullException(nameof(handler));
        Options = options ?? new DeepLinkMapOptions();
    }

    /// <summary>
    /// Creates a map that opens a Shell route. Tokens such as <c>{id}</c> are replaced from the match.
    /// </summary>
    public DeepLinkMapRegistration(string template, string shellRoute, DeepLinkMapOptions? options = null)
    {
        Template = template ?? throw new ArgumentNullException(nameof(template));
        ShellRoute = shellRoute ?? throw new ArgumentNullException(nameof(shellRoute));
        Options = options ?? new DeepLinkMapOptions();
    }

    internal DeepLinkMapRegistration(
        string template,
        Func<DeepLinkRoute, CancellationToken, Task>? handler,
        string? shellRoute,
        DeepLinkMapOptions options)
    {
        Template = template;
        Handler = handler;
        ShellRoute = shellRoute;
        Options = options;
    }

    /// <summary>
    /// Path template, for example <c>/orders/{id}</c>.
    /// </summary>
    public string Template { get; }

    /// <summary>
    /// Custom handler. When set, it wins over <see cref="ShellRoute"/>.
    /// </summary>
    public Func<DeepLinkRoute, CancellationToken, Task>? Handler { get; }

    /// <summary>
    /// Shell path template, for example <c>order?id={id}</c>.
    /// </summary>
    public string? ShellRoute { get; }

    /// <summary>
    /// Per-route options.
    /// </summary>
    public DeepLinkMapOptions Options { get; }
}
