namespace Plugin.Maui.DeepLinks.Sample;

public partial class App : Application
{
    public App()
    {
        InitializeComponent();
    }

    protected override Window CreateWindow(IActivationState? activationState)
    {
        var window = new Window(new AppShell());
        DeepLinks.MarkReady();
        return window;
    }
}
