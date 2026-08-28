namespace Plugin.Maui.DeepLinks.Sample;

public partial class CatalogPage : ContentPage
{
    public CatalogPage()
    {
        InitializeComponent();
    }

    async void OnOrder(object? sender, EventArgs e) =>
        await DeepLinks.HandleAsync("https://example.com/orders/123");
}
