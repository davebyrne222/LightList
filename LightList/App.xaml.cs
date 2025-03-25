namespace LightList;

public partial class App : Application
{
    private AppShell _shell;
    public App(AppShell shell)
    {
        InitializeComponent();
        _shell = shell;
    }

    protected override Window CreateWindow(IActivationState? activationState)
    {
        // TODO: remove / make this work properly
        if (Current != null)
            Current.UserAppTheme = AppTheme.Light;

        return new Window(_shell);
    }
}