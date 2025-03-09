namespace LightList;

public partial class App : Application
{
    public App()
    {
        InitializeComponent();
    }

    protected override Window CreateWindow(IActivationState? activationState)
    {
        // TODO: remove / make this work properly
        if (Current != null)
            Current.UserAppTheme = AppTheme.Light;

        return new Window(new AppShell());
    }
}