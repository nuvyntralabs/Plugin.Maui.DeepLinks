namespace Plugin.Maui.DeepLinks.Sample;

public partial class LoginPage : ContentPage
{
    public LoginPage()
    {
        InitializeComponent();
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        var pending = DeepLinks.Current.Pending;
        PendingLabel.Text = pending is null
            ? "No pending link."
            : $"Will restore{Environment.NewLine}{pending.Uri}";
    }

    void OnSignIn(object? sender, EventArgs e) => Session.SignIn();
}
