using System.Collections.ObjectModel;
using System.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using LightList.Services;
using LightList.Utils;

namespace LightList.ViewModels;

public class BaseTasksViewModel: INotifyPropertyChanged
{
    protected ITaskViewModelFactory TaskViewModelFactory { get; }
    protected ITasksService TasksService { get; }
    protected IMessenger Messenger { get; }
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

    public BaseTasksViewModel(ITaskViewModelFactory taskViewModelFactory, ITasksService tasksService, IMessenger messenger)
    {
        TaskViewModelFactory = taskViewModelFactory;
        TasksService = tasksService;
        Messenger = messenger;
    }
    
    private void OnPropertyChanged(string propertyName)
    {
        Logger.Log($"PropertyChanged: {propertyName}");
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}