namespace Plugin.Maui.DeepLinks.Sample;

public partial class AccountPage : ContentPage, IQueryAttributable
{
    public AccountPage()
    {
        InitializeComponent();
    }

    public void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        var section = query.TryGetValue("section", out var value) ? value?.ToString() : "(missing)";
        DetailLabel.Text = $"Section: {section}";
    }

    async void OnRestore(object? sender, EventArgs e) =>
        await DeepLinks.RestoreNavigationStackAsync();
}
