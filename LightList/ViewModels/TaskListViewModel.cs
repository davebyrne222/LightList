using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows.Input;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using LightList.Messages;
using LightList.Services;
using LightList.Views.Components;

namespace LightList.ViewModels;

public class TaskListViewModel: INotifyPropertyChanged
{
    
    private ObservableCollection<TaskViewModel> _tasks;
    public ObservableCollection<TaskViewModel> Tasks
    {
        get => _tasks;
        set
        {
            Console.WriteLine($"----[TaskListViewModel] Tasks Changing: {_tasks.Count} - {value.Count}");
            if (_tasks != value)
            {
                _tasks = value;
                Debug.WriteLine($"----[TaskListViewModel] Tasks Changed: {_tasks.Count}");
                OnPropertyChanged(nameof(Tasks));
            }
        }
    }
    
    public ICommand SelectTaskCommand { get; }
    private ITaskViewModelFactory TaskViewModelFactory { get; }
    private ITasksService TasksService { get; }
    private readonly IMessenger _messenger;
    
    public TaskListViewModel(ITaskViewModelFactory taskViewModelFactory, ITasksService tasksService, IMessenger messenger)
    {
        TaskViewModelFactory = taskViewModelFactory;
        TasksService = tasksService;
        _messenger = messenger;
        SelectTaskCommand = new AsyncRelayCommand<TaskViewModel>(SelectTaskAsync);
        // Tasks = new ObservableCollection<TaskViewModel>();
        
        _messenger.Register<TaskSavedMessage>(this, (recipient, message) =>
        {
            OnTaskSaved(message.Value);
        });

        _messenger.Register<TaskDeletedMessage>(this, (recipient, message) =>
        {
            OnTaskDeleted(message.Value);
        });
    }
    
    public event PropertyChangedEventHandler PropertyChanged;
    protected virtual void OnPropertyChanged(string propertyName)
    {
        Console.WriteLine($"----[TaskListViewModel] PropertyChanged: {propertyName}: {_tasks.Count}");
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
    
    private async System.Threading.Tasks.Task? SelectTaskAsync(TaskViewModel task)
    {
        Console.WriteLine($"----[TaskListViewModel] Task Selected: {task.Text}");
        if (task != null)
            await Shell.Current.GoToAsync($"{nameof(Views.TaskPage)}?load={task.Id}");
    }

    void OnTaskDeleted(string taskId)
    {
        TaskViewModel? matchedTask = this.Tasks.FirstOrDefault(n => n.Id == taskId);

        if (matchedTask != null)
            Tasks.Remove(matchedTask);
    }

    void OnTaskSaved(string taskId)
    {
        TaskViewModel? matchedTask = Tasks.FirstOrDefault(n => n.Id == taskId);

        if (matchedTask != null)
        {
            matchedTask.Reload();
            Tasks.Move(Tasks.IndexOf(matchedTask), 0);
        }
            
        else 
            Tasks.Insert(0, TaskViewModelFactory.Create(TasksService.GetTask(taskId)));
    }
}