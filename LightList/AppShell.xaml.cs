using LightList.Services;
using LightList.Utils;
using LightList.Views;

namespace LightList;

public partial class AppShell : Shell
{
    public AppShell()
    {
        Logger.Log("Initializing");
        InitializeComponent();
        RegisterRoutes();
    }

    private void RegisterRoutes()
    {
        Routing.RegisterRoute(nameof(TaskPage), typeof(TaskPage));
        Routing.RegisterRoute(nameof(AllTasksPage), typeof(AllTasksPage));
        Routing.RegisterRoute(nameof(TasksByDueDatePage), typeof(TasksByDueDatePage));
        Routing.RegisterRoute(nameof(TasksByLabelPage), typeof(TasksByLabelPage));
        Routing.RegisterRoute(nameof(LoginPage), typeof(LoginPage));
    }

    private async Task CloseFlyout()
    {
        Current.FlyoutBehavior = FlyoutBehavior.Disabled;
        await Task.Delay(1000); // Small delay to allow animation
        Current.FlyoutBehavior = FlyoutBehavior.Flyout;
    }

    private async void OnLoginClicked(object sender, EventArgs e)
    {
        Logger.Log("Navigating to the login page");
        CloseFlyout();
        await Current.GoToAsync("LoginPage");
    }

    private async void OnLogoutClicked(object sender, EventArgs e)
    {
        throw new NotImplementedException();
    }
}