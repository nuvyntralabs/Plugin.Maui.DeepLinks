namespace Plugin.Maui.DeepLinks.Sample;

public partial class HomePage : ContentPage
{
    public HomePage()
    {
        InitializeComponent();
        DeepLinks.Current.Received += OnReceived;
        DeepLinks.Current.Navigated += OnNavigated;
        DeepLinks.Current.Deferred += OnDeferred;
        DeepLinks.Current.Unhandled += OnUnhandled;
        Session.Changed += (_, _) => UpdateStatus();
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        UpdateStatus();
    }

    void UpdateStatus()
    {
        var pending = DeepLinks.Current.Pending;
        StatusLabel.Text = Session.IsLoggedIn
            ? "Signed in"
            : pending is null
                ? "Signed out"
                : $"Signed out · pending {pending.Uri}";
    }

    async void OnSignIn(object? sender, EventArgs e)
    {
        Session.SignIn();
        UpdateStatus();
        await Task.CompletedTask;
    }

    void OnSignOut(object? sender, EventArgs e)
    {
        Session.SignOut();
        UpdateStatus();
    }

    async void OnHttpsOrder(object? sender, EventArgs e) =>
        await DeepLinks.HandleAsync("https://example.com/orders/123");

    async void OnSchemeOrder(object? sender, EventArgs e) =>
        await DeepLinks.HandleAsync("myapp://orders/456");

    async void OnAccount(object? sender, EventArgs e) =>
        await DeepLinks.HandleAsync("https://example.com/account/settings");

    async void OnCatalog(object? sender, EventArgs e) =>
        await Shell.Current.GoToAsync("catalog");

    async void OnRestore(object? sender, EventArgs e) =>
        await DeepLinks.RestoreNavigationStackAsync();

    void OnReceived(object? sender, DeepLinkReceivedEventArgs e) =>
        SetLast($"Received {e.Link.Kind} {e.Link.Launch}{Environment.NewLine}{e.Link.Uri}");

    void OnNavigated(object? sender, DeepLinkNavigatedEventArgs e) =>
        SetLast($"Navigated {e.Result.Route?.Template}{Environment.NewLine}{e.Result.Link?.Uri}");

    void OnDeferred(object? sender, DeepLinkDeferredEventArgs e) =>
        SetLast($"Deferred ({e.Reason}){Environment.NewLine}{e.Link.Uri}");

    void OnUnhandled(object? sender, DeepLinkUnhandledEventArgs e) =>
        SetLast($"Unhandled{Environment.NewLine}{e.Link.Uri}");

    void SetLast(string text)
    {
        MainThread.BeginInvokeOnMainThread(() =>
        {
            LastLabel.Text = text;
            UpdateStatus();
        });
    }
}
