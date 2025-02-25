using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.Windows.Input;
using LightList.Services;

namespace LightList.ViewModels;

public class TasksViewModel: IQueryAttributable
{
    public ObservableCollection<TaskViewModel> AllTasks { get; }
    public ICommand NewCommand { get; }
    public ICommand SelectTaskCommand { get; }
    private ITaskViewModelFactory TaskViewModelFactory { get; }
    private ITasksService TasksService { get; }
    
    public TasksViewModel(ITaskViewModelFactory taskViewModelFactory, ITasksService tasksService)
    {
        TaskViewModelFactory = taskViewModelFactory;
        TasksService = tasksService;
        AllTasks = new ObservableCollection<TaskViewModel>(TasksService.GetTasks().Select(n => TaskViewModelFactory.Create(n)));
        NewCommand = new AsyncRelayCommand(NewTaskAsync);
        SelectTaskCommand = new AsyncRelayCommand<TaskViewModel>(SelectTaskAsync);
    }
    
    private async System.Threading.Tasks.Task NewTaskAsync()
    {
        await Shell.Current.GoToAsync(nameof(Views.TaskPage));
    }

    private async System.Threading.Tasks.Task SelectTaskAsync(TaskViewModel task)
    {
        if (task != null)
            await Shell.Current.GoToAsync($"{nameof(Views.TaskPage)}?load={task.Identifier}");
    }
    
    void IQueryAttributable.ApplyQueryAttributes(IDictionary<string, object> query)
    {
        if (query.ContainsKey("deleted"))
        {
            string taskId = query["deleted"].ToString();
            TaskViewModel matchedTask = AllTasks.Where((n) => n.Identifier == taskId).FirstOrDefault();

            // If task exists, delete it
            if (matchedTask != null)
                AllTasks.Remove(matchedTask);
        }
        else if (query.ContainsKey("saved"))
        {
            string taskId = query["saved"].ToString();
            TaskViewModel matchedTask = AllTasks.Where((n) => n.Identifier == taskId).FirstOrDefault();

            // If task is found, update it
            if (matchedTask != null)
            {
                matchedTask.Reload();
                AllTasks.Move(AllTasks.IndexOf(matchedTask), 0);
            }
            
            // If task isn't found, it's new; add it.
            else 
                AllTasks.Insert(0, TaskViewModelFactory.Create(TasksService.GetTask(taskId)));
        }
    } 
}