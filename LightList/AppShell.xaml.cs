namespace LightList;

public partial class AppShell : Shell
{
    public AppShell()
    {
        InitializeComponent();
        
        Routing.RegisterRoute(nameof(Views.AllTasksPage), typeof(Views.AllTasksPage));
        Routing.RegisterRoute(nameof(Views.TaskPage), typeof(Views.TaskPage));
        // Routing.RegisterRoute(nameof(Views.TasksByDueDatePage), typeof(Views.TasksByDueDatePage));
        // Routing.RegisterRoute(nameof(Views.TasksByLabelPage), typeof(Views.TasksByLabelPage));
    }
}