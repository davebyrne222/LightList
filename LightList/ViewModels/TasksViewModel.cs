using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Windows.Input;
using CommunityToolkit.Mvvm.Messaging;
using LightList.Messages;
using LightList.Services;

namespace LightList.ViewModels;

public class TasksViewModel: IQueryAttributable
{
    public ObservableCollection<TaskViewModel> AllTasks { get; }
    public ICommand SelectTaskCommand { get; }
    private ITaskViewModelFactory TaskViewModelFactory { get; }
    private ITasksService TasksService { get; }
    private readonly IMessenger _messenger;
    
    public TasksViewModel(ITaskViewModelFactory taskViewModelFactory, ITasksService tasksService, IMessenger messenger)
    {
        TaskViewModelFactory = taskViewModelFactory;
        TasksService = tasksService;
        _messenger = messenger;
        AllTasks = new ObservableCollection<TaskViewModel>(TasksService.GetTasks().Select(n => TaskViewModelFactory.Create(n)));
        SelectTaskCommand = new AsyncRelayCommand<TaskViewModel>(SelectTaskAsync);
        
        _messenger.Register<TaskSavedMessage>(this, (recipient, message) =>
        {
            OnTaskSaved(message.Value);
        });

        _messenger.Register<TaskDeletedMessage>(this, (recipient, message) =>
        {
            OnTaskDeleted(message.Value);
        });
    }
    
    private async System.Threading.Tasks.Task SelectTaskAsync(TaskViewModel task)
    {
        if (task != null)
            await Shell.Current.GoToAsync($"{nameof(Views.TaskPage)}?load={task.Id}");
    }

    void OnTaskDeleted(string taskId)
    {
        Console.WriteLine($"TaskDeletedMessage: {taskId} deleted");
        
        TaskViewModel? matchedTask = AllTasks.FirstOrDefault(n => n.Id == taskId);

        // If task exists, delete it
        if (matchedTask != null)
            AllTasks.Remove(matchedTask);
    }

    void OnTaskSaved(string taskId)
    {
        Console.WriteLine($"TaskSavedMessage: {taskId} saved");

        TaskViewModel? matchedTask = AllTasks.FirstOrDefault(n => n.Id == taskId);

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
    
    void IQueryAttributable.ApplyQueryAttributes(IDictionary<string, object> query) { } 
}