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

    private void OpenMenu()
    {
        Shell.Current.FlyoutIsPresented = true;
    }

    private async void AddTask()
    {
        await Shell.Current.GoToAsync($"///{nameof(Views.TaskPage)}");
    }
    
    private async void ViewAllTasks()
    {
        await Shell.Current.GoToAsync($"///{nameof(Views.AllTasksPage)}");
    }
    
    private async void ViewTasksByDueDate()
    {
        await Shell.Current.GoToAsync($"///{nameof(Views.AllTasksPage)}");
    }
    
    private async void ViewTasksByLabel()
    {
        await Shell.Current.GoToAsync($"///{nameof(Views.AllTasksPage)}");
    }
}