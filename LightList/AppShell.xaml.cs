namespace LightList;

public partial class AppShell : Shell
{
    public AppShell()
    {
        InitializeComponent();
        
        Routing.RegisterRoute(nameof(Views.TaskPage), typeof(Views.TaskPage));
    }
}