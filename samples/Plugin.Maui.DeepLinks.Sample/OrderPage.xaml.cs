namespace Plugin.Maui.DeepLinks.Sample;

public partial class OrderPage : ContentPage, IQueryAttributable
{
    public OrderPage()
    {
        InitializeComponent();
    }

    public void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        var id = query.TryGetValue("id", out var value) ? value?.ToString() : "(missing)";
        DetailLabel.Text = $"Order {id}";
    }

    async void OnRestore(object? sender, EventArgs e) =>
        await DeepLinks.RestoreNavigationStackAsync();
}
