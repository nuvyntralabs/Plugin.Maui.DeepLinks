namespace Plugin.Maui.DeepLinks;

static class DeepLinkParser
{
    public static bool TryParse(
        string value,
        DeepLinkLaunch launch,
        DeepLinksOptions options,
        [NotNullWhen(true)] out DeepLink? link)
    {
        link = null;
        if (string.IsNullOrWhiteSpace(value))
        {
            return false;
        }

        if (!Uri.TryCreate(value.Trim(), UriKind.Absolute, out var uri))
        {
            return false;
        }

        return TryParse(uri, launch, options, native: null, out link);
    }

    public static bool TryParse(
        Uri uri,
        DeepLinkLaunch launch,
        DeepLinksOptions options,
        object? native,
        [NotNullWhen(true)] out DeepLink? link)
    {
        ArgumentNullException.ThrowIfNull(uri);
        ArgumentNullException.ThrowIfNull(options);

        link = null;
        if (!IsCandidate(uri, options))
        {
            return false;
        }

        var kind = ResolveKind(uri);
        var path = ExtractPath(uri, options);
        var query = ParseQuery(uri);
        link = new DeepLink(uri, kind, launch, path, query, DateTimeOffset.UtcNow, native);
        return true;
    }

    public static bool IsCandidate(Uri uri, DeepLinksOptions options)
    {
        if (!uri.IsAbsoluteUri)
        {
            return false;
        }

        var scheme = uri.Scheme;
        if (IsHttp(scheme))
        {
            return HostMatches(uri.Host, options.Hosts);
        }

        if (options.CustomSchemes.Count == 0)
        {
            return true;
        }

        foreach (var allowed in options.CustomSchemes)
        {
            if (scheme.Equals(allowed, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
        }

        return false;
    }

    public static DeepLinkKind ResolveKind(Uri uri)
    {
        if (!IsHttp(uri.Scheme))
        {
            return DeepLinkKind.CustomScheme;
        }

#if ANDROID
        return DeepLinkKind.AppLink;
#elif IOS
        return DeepLinkKind.UniversalLink;
#else
        return DeepLinkKind.AppLink;
#endif
    }

    public static string ExtractPath(Uri uri, DeepLinksOptions options)
    {
        if (IsHttp(uri.Scheme))
        {
            return RouteMatcher.NormalizePath(string.IsNullOrEmpty(uri.AbsolutePath) ? "/" : uri.AbsolutePath);
        }

        var absolutePath = string.IsNullOrEmpty(uri.AbsolutePath) ? "/" : uri.AbsolutePath;
        var host = uri.Host;

        if (string.IsNullOrEmpty(host))
        {
            return RouteMatcher.NormalizePath(absolutePath);
        }

        if (HostMatches(host, options.Hosts) && options.Hosts.Count > 0)
        {
            return RouteMatcher.NormalizePath(absolutePath);
        }

        var combined = "/" + host + (absolutePath == "/" ? string.Empty : absolutePath);
        return RouteMatcher.NormalizePath(combined);
    }

    public static Dictionary<string, string> ParseQuery(Uri uri)
    {
        var result = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        var query = uri.Query;
        if (string.IsNullOrEmpty(query))
        {
            return result;
        }

        var text = query[0] == '?' ? query[1..] : query;
        foreach (var part in text.Split('&', StringSplitOptions.RemoveEmptyEntries))
        {
            var separator = part.IndexOf('=');
            var rawKey = separator < 0 ? part : part[..separator];
            var rawValue = separator < 0 ? string.Empty : part[(separator + 1)..];
            var key = Decode(rawKey);
            if (string.IsNullOrEmpty(key))
            {
                continue;
            }

            result[key] = Decode(rawValue);
        }

        return result;
    }

    public static bool HostMatches(string host, IList<string> allowed)
    {
        if (allowed.Count == 0)
        {
            return true;
        }

        foreach (var pattern in allowed)
        {
            if (string.IsNullOrWhiteSpace(pattern))
            {
                continue;
            }

            if (pattern.StartsWith("*.", StringComparison.Ordinal))
            {
                var domain = pattern[2..];
                if (host.Equals(domain, StringComparison.OrdinalIgnoreCase)
                    || host.EndsWith("." + domain, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }
            else if (host.Equals(pattern, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
        }

        return false;
    }

    static bool IsHttp(string scheme) =>
        scheme.Equals(Uri.UriSchemeHttps, StringComparison.OrdinalIgnoreCase)
        || scheme.Equals(Uri.UriSchemeHttp, StringComparison.OrdinalIgnoreCase);

    static string Decode(string value)
    {
        if (string.IsNullOrEmpty(value))
        {
            return string.Empty;
        }

        return Uri.UnescapeDataString(value.Replace('+', ' '));
    }
}
