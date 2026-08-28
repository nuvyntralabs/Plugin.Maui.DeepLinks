namespace Plugin.Maui.DeepLinks;

sealed class RouteParameter
{
    public RouteParameter(string name, bool optional, bool catchAll, string? constraint)
    {
        Name = name;
        Optional = optional;
        CatchAll = catchAll;
        Constraint = constraint;
    }

    public string Name { get; }
    public bool Optional { get; }
    public bool CatchAll { get; }
    public string? Constraint { get; }
}

sealed class RoutePattern
{
    public RoutePattern(
        string template,
        string normalized,
        Regex regex,
        IReadOnlyList<RouteParameter> parameters,
        int staticSegmentCount,
        int requiredParameterCount,
        bool hasCatchAll)
    {
        Template = template;
        Normalized = normalized;
        Regex = regex;
        Parameters = parameters;
        StaticSegmentCount = staticSegmentCount;
        RequiredParameterCount = requiredParameterCount;
        HasCatchAll = hasCatchAll;
    }

    public string Template { get; }
    public string Normalized { get; }
    public Regex Regex { get; }
    public IReadOnlyList<RouteParameter> Parameters { get; }
    public int StaticSegmentCount { get; }
    public int RequiredParameterCount { get; }
    public bool HasCatchAll { get; }

    public bool TryMatch(string path, [NotNullWhen(true)] out Dictionary<string, string>? values)
    {
        values = null;
        var match = Regex.Match(path);
        if (!match.Success)
        {
            return false;
        }

        var captured = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        foreach (var parameter in Parameters)
        {
            var group = match.Groups[parameter.Name];
            if (group.Success && !string.IsNullOrEmpty(group.Value))
            {
                captured[parameter.Name] = Uri.UnescapeDataString(group.Value);
            }
        }

        values = captured;
        return true;
    }
}

static class RouteMatcher
{
    public static RoutePattern Compile(string template, bool caseInsensitive = true)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(template);

        var normalized = NormalizePath(template);
        var options = RegexOptions.CultureInvariant | RegexOptions.Compiled;
        if (caseInsensitive)
        {
            options |= RegexOptions.IgnoreCase;
        }

        if (normalized == "/")
        {
            return new RoutePattern(
                template,
                normalized,
                new Regex("^/?$", options),
                [],
                staticSegmentCount: 0,
                requiredParameterCount: 0,
                hasCatchAll: false);
        }

        var segments = normalized.Trim('/').Split('/', StringSplitOptions.RemoveEmptyEntries);
        var builder = new StringBuilder("^");
        var parameters = new List<RouteParameter>();
        var staticCount = 0;
        var requiredParams = 0;
        var hasCatchAll = false;

        foreach (var segment in segments)
        {
            if (TryParseParameter(segment, out var name, out var optional, out var catchAll, out var constraint))
            {
                parameters.Add(new RouteParameter(name, optional, catchAll, constraint));
                var body = catchAll
                    ? $"(?<{name}>.+)"
                    : $"(?<{name}>{ConstraintRegex(constraint)})";

                if (optional)
                {
                    builder.Append("(?:/").Append(body).Append(')');
                    builder.Append('?');
                }
                else
                {
                    builder.Append('/').Append(body);
                    if (!catchAll)
                    {
                        requiredParams++;
                    }
                }

                hasCatchAll |= catchAll;
            }
            else
            {
                staticCount++;
                builder.Append('/').Append(Regex.Escape(segment));
            }
        }

        builder.Append("/?$");
        return new RoutePattern(
            template,
            normalized,
            new Regex(builder.ToString(), options),
            parameters,
            staticCount,
            requiredParams,
            hasCatchAll);
    }

    public static string NormalizePath(string path)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);

        var trimmed = path.Trim();
        var query = IndexOfQueryOrFragment(trimmed);
        if (query >= 0)
        {
            trimmed = trimmed[..query];
        }

        if (!trimmed.StartsWith('/'))
        {
            trimmed = "/" + trimmed;
        }

        if (trimmed.Length > 1)
        {
            trimmed = trimmed.TrimEnd('/');
        }

        return string.IsNullOrEmpty(trimmed) ? "/" : trimmed;
    }

    static int IndexOfQueryOrFragment(string value)
    {
        var depth = 0;
        for (var i = 0; i < value.Length; i++)
        {
            var character = value[i];
            if (character == '{')
            {
                depth++;
            }
            else if (character == '}' && depth > 0)
            {
                depth--;
            }
            else if (depth == 0 && (character == '?' || character == '#'))
            {
                return i;
            }
        }

        return -1;
    }

    public static int CompareSpecificity(RoutePattern left, RoutePattern right)
    {
        var staticCompare = right.StaticSegmentCount.CompareTo(left.StaticSegmentCount);
        if (staticCompare != 0)
        {
            return staticCompare;
        }

        var requiredCompare = right.RequiredParameterCount.CompareTo(left.RequiredParameterCount);
        if (requiredCompare != 0)
        {
            return requiredCompare;
        }

        return left.HasCatchAll.CompareTo(right.HasCatchAll);
    }

    static bool TryParseParameter(
        string segment,
        [NotNullWhen(true)] out string? name,
        out bool optional,
        out bool catchAll,
        out string? constraint)
    {
        name = null;
        optional = false;
        catchAll = false;
        constraint = null;

        if (segment.Length < 3 || segment[0] != '{' || segment[^1] != '}')
        {
            return false;
        }

        var inner = segment[1..^1];
        if (inner.StartsWith('*'))
        {
            catchAll = true;
            inner = inner[1..];
        }

        if (inner.EndsWith('?'))
        {
            optional = true;
            inner = inner[..^1];
        }

        var colon = inner.IndexOf(':');
        if (colon >= 0)
        {
            name = inner[..colon];
            constraint = inner[(colon + 1)..];
            if (constraint.EndsWith('?'))
            {
                optional = true;
                constraint = constraint[..^1];
            }
        }
        else
        {
            name = inner;
        }

        return !string.IsNullOrWhiteSpace(name);
    }

    static string ConstraintRegex(string? constraint) => constraint?.ToLowerInvariant() switch
    {
        "int" or "long" => @"-?\d+",
        "guid" => @"[0-9a-fA-F]{8}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{12}",
        _ => @"[^/]+"
    };
}
