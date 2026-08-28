namespace Plugin.Maui.DeepLinks;

/// <summary>
/// A matched route. Path parameters and query values are available through the indexer.
/// </summary>
/// <example>
/// <code>
/// DeepLinks.Map("/orders/{id}", async route =>
/// {
///     await Shell.Current.GoToAsync($"order?id={route["id"]}");
/// });
/// </code>
/// </example>
public sealed class DeepLinkRoute
{
    readonly IReadOnlyDictionary<string, string> _parameters;
    readonly IReadOnlyDictionary<string, string> _query;

    /// <summary>
    /// Creates a matched route.
    /// </summary>
    public DeepLinkRoute(
        DeepLink link,
        string template,
        IReadOnlyDictionary<string, string> parameters,
        bool requiresAuthentication)
    {
        Link = link ?? throw new ArgumentNullException(nameof(link));
        Template = template ?? throw new ArgumentNullException(nameof(template));
        _parameters = parameters ?? throw new ArgumentNullException(nameof(parameters));
        _query = link.Query;
        RequiresAuthentication = requiresAuthentication;
    }

    /// <summary>
    /// Incoming link that matched.
    /// </summary>
    public DeepLink Link { get; }

    /// <summary>
    /// Template that matched, for example <c>/orders/{id}</c>.
    /// </summary>
    public string Template { get; }

    /// <summary>
    /// Normalized path, for example <c>/orders/123</c>.
    /// </summary>
    public string Path => Link.Path;

    /// <summary>
    /// Original URI.
    /// </summary>
    public Uri Uri => Link.Uri;

    /// <summary>
    /// Captured path parameters.
    /// </summary>
    public IReadOnlyDictionary<string, string> Parameters => _parameters;

    /// <summary>
    /// Query string values from the URI.
    /// </summary>
    public IReadOnlyDictionary<string, string> Query => _query;

    /// <summary>
    /// Whether this map requires a signed-in user.
    /// </summary>
    public bool RequiresAuthentication { get; }

    /// <summary>
    /// Path parameter first, then query. Missing keys return an empty string.
    /// </summary>
    public string this[string name]
    {
        get
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(name);
            return TryGet(name, out var value) ? value : string.Empty;
        }
    }

    /// <summary>
    /// Tries path parameters, then query values.
    /// </summary>
    public bool TryGet(string name, [NotNullWhen(true)] out string? value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        if (_parameters.TryGetValue(name, out var parameter) && !string.IsNullOrEmpty(parameter))
        {
            value = parameter;
            return true;
        }

        if (_query.TryGetValue(name, out var query) && !string.IsNullOrEmpty(query))
        {
            value = query;
            return true;
        }

        value = null;
        return false;
    }

    /// <inheritdoc />
    public override string ToString() => $"{Template} <- {Path}";
}
