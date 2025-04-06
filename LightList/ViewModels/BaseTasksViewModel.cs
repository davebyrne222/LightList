using System.Collections.ObjectModel;
using System.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using LightList.Services;
using LightList.Utils;

namespace LightList.ViewModels;

public class BaseTasksViewModel : INotifyPropertyChanged
{
    private readonly ILogger _logger;
    private ObservableCollection<TaskViewModel> _tasks = new();

    public BaseTasksViewModel(
        ITaskViewModelFactory taskViewModelFactory,
        ITasksService tasksService,
        IMessenger messenger,
        ILogger logger)
    {
        TaskViewModelFactory = taskViewModelFactory;
        TasksService = tasksService;
        Messenger = messenger;
        _logger = logger;
    }

    protected ITaskViewModelFactory TaskViewModelFactory { get; }
    protected ITasksService TasksService { get; }
    protected IMessenger Messenger { get; }

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

    private void OnPropertyChanged(string propertyName)
    {
        _logger.Debug($"PropertyChanged: {propertyName}");
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}