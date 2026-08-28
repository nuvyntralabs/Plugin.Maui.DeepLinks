namespace Plugin.Maui.DeepLinks.Sample;

static class Session
{
    public static bool IsLoggedIn { get; set; }

    public static event EventHandler? Changed;

    public static void SignIn()
    {
        IsLoggedIn = true;
        Changed?.Invoke(null, EventArgs.Empty);
        DeepLinks.NotifyAuthenticated();
    }

    public static void SignOut()
    {
        IsLoggedIn = false;
        Changed?.Invoke(null, EventArgs.Empty);
        DeepLinks.Current.NotifyUnauthenticated();
    }
}
