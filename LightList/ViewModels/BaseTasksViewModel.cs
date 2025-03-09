using System.Collections.ObjectModel;
using System.ComponentModel;
using LightList.Services;
using LightList.Utils;

namespace LightList.ViewModels;

public class BaseTasksViewModel: INotifyPropertyChanged
{
    protected ITaskViewModelFactory TaskViewModelFactory { get; }
    protected ITasksService TasksService { get; }
    private ObservableCollection<TaskViewModel> _tasks = new();
    public ObservableCollection<TaskViewModel> AllTasks
    {
        get => _tasks;
        set
        {
            if (_tasks != value)
            {
                _tasks = value;
                OnPropertyChanged(nameof(AllTasks));
            }
        }
    }
    public event PropertyChangedEventHandler? PropertyChanged = delegate { };

    public BaseTasksViewModel(ITaskViewModelFactory taskViewModelFactory, ITasksService tasksService)
    {
        TaskViewModelFactory = taskViewModelFactory;
        TasksService = tasksService;
    }
    
    private void OnPropertyChanged(string propertyName)
    {
        Logger.Log($"PropertyChanged: {propertyName}");
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}