namespace Plugin.Maui.DeepLinks.Sample;

public partial class AppShell : Shell
{
    public AppShell()
    {
        InitializeComponent();
        Routing.RegisterRoute("catalog", typeof(CatalogPage));
    }
}
