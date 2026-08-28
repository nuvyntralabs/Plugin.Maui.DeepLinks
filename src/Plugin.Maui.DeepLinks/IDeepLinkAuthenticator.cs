namespace Plugin.Maui.DeepLinks;

/// <summary>
/// Tells the router whether the user may open authentication-required links.
/// </summary>
public interface IDeepLinkAuthenticator
{
    /// <summary>
    /// <c>true</c> when the user is signed in.
    /// </summary>
    bool IsAuthenticated { get; }
}

/// <summary>
/// Wraps a <see cref="Func{TResult}"/> as <see cref="IDeepLinkAuthenticator"/>.
/// </summary>
public sealed class DelegateDeepLinkAuthenticator : IDeepLinkAuthenticator
{
    readonly Func<bool> _isAuthenticated;

    /// <summary>
    /// Creates an authenticator from a delegate.
    /// </summary>
    public DelegateDeepLinkAuthenticator(Func<bool> isAuthenticated)
    {
        _isAuthenticated = isAuthenticated ?? throw new ArgumentNullException(nameof(isAuthenticated));
    }

    /// <inheritdoc />
    public bool IsAuthenticated => _isAuthenticated();
}
