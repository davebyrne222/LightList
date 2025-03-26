using LightList.Data;
using LightList.Services;
using LightList.Utils;

namespace LightList;

public partial class App : Application
{
    private AppShell _shell;
    public App(AppShell shell)
    {
        Logger.Log("App initializing");
        InitializeComponent();
        _shell = shell;
        Logger.Log("App initialized");
    }

    protected override Window CreateWindow(IActivationState? activationState)
    {
        // TODO: remove / make this work properly
        if (Current != null)
            Current.UserAppTheme = AppTheme.Light;

        return new Window(_shell);
    }
}