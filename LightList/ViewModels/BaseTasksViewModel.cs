using System.Collections.ObjectModel;
using System.Collections.Specialized;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using LightList.Messages;
using LightList.Services;
using LightList.Utils;

namespace LightList.ViewModels;

public partial class BaseTasksViewModel : ObservableObject
{
    private readonly ILogger _logger;
    [ObservableProperty] private ObservableCollection<TaskViewModel> _allTasks = new();

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

        Messenger.Register<TasksSyncedMessage>(this, async (recipient, _) => { await GetTasks(); });
    }

    protected ITaskViewModelFactory TaskViewModelFactory { get; }
    protected ITasksService TasksService { get; }
    protected IMessenger Messenger { get; }

    protected async Task OnNavigatedTo()
    {
        _logger.Debug("OnNavigatedTo");
        await GetTasks();
    }

    protected async Task GetTasks()
    {
        _logger.Debug("Retrieving tasks");
        try
        {
            var tasks = await TasksService.GetTasks();
            AllTasks = new ObservableCollection<TaskViewModel>(tasks.Select(n => TaskViewModelFactory.Create(n)));
            _logger.Debug($"Retrieved {AllTasks.Count} tasks");
        }
        catch (Exception ex)
        {
            _logger.Error($"Failed to get tasks: {ex.GetType()} - {ex.Message}");
            throw; // TODO: await DisplayAlert("Error retrieving tasks. Please try again", ex.Message, "OK");
        }
    }

    partial void OnAllTasksChanged(
        ObservableCollection<TaskViewModel>? oldValue,
        ObservableCollection<TaskViewModel> newValue)
    {
        _logger.Debug("All tasks changed");

        if (oldValue != null)
            oldValue.CollectionChanged -= AllTasks_CollectionChanged;

        newValue.CollectionChanged += AllTasks_CollectionChanged;
    }

    protected virtual void AllTasks_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        // Do nothing - derived class to override
    }
}