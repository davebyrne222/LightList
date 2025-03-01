using System.Diagnostics;
using System.Windows.Input;

namespace LightList.ViewModels;

public class NavBarViewModel
{
    public ICommand OpenMenuCommand { get; }
    public ICommand AddTaskCommand { get; }
    public ICommand AllTasksCommand { get; }
    public ICommand TasksByDueDateCommand { get; }
    public ICommand TasksByLabelCommand { get; }

    public NavBarViewModel()
    {
        OpenMenuCommand = new Command(OpenMenu);
        AddTaskCommand = new Command(AddTask);
        AllTasksCommand = new Command(ViewAllTasks);
        TasksByDueDateCommand = new Command(ViewTasksByDueDate);
        TasksByLabelCommand = new Command(ViewTasksByLabel);
    }

    private static void OpenMenu()
    {
        Debug.WriteLine("---[NavBarViewModel.OpenMenu] Opening Flyout");
        Shell.Current.FlyoutIsPresented = true;
    }

    private async void AddTask()
    {
        if (Shell.Current.CurrentPage is Views.TaskPage)
            return;
        
        Debug.WriteLine($"---[NavBarViewModel.AddTask] Navigating to {nameof(Views.TaskPage)}");
        await Shell.Current.GoToAsync(nameof(Views.TaskPage));
    }
    
    private async void ViewAllTasks()
    {
        if (Shell.Current.CurrentPage is Views.AllTasksPage)
            return;
        
        Debug.WriteLine($"---[NavBarViewModel.ViewAllTasks] Navigating to {nameof(Views.AllTasksPage)}");
        await Shell.Current.GoToAsync($"//{nameof(Views.AllTasksPage)}", true);
    }
    
    private async void ViewTasksByDueDate()
    {
        if (Shell.Current.CurrentPage is Views.TasksByDueDatePage)
            return;
        
        Debug.WriteLine($"---[NavBarViewModel.ViewTasksByDueDate] Navigating to {nameof(Views.TasksByDueDatePage)}");
        await Shell.Current.GoToAsync($"//{nameof(Views.TasksByDueDatePage)}", true);
    }
    
    private async void ViewTasksByLabel()
    {
        if (Shell.Current.CurrentPage is Views.TasksByLabelPage)
            return;
        
        Debug.WriteLine($"---[NavBarViewModel.ViewTasksByLabel] Navigating to {nameof(Views.TasksByLabelPage)}");
        await Shell.Current.GoToAsync($"//{nameof(Views.TasksByLabelPage)}", true);
    }
}