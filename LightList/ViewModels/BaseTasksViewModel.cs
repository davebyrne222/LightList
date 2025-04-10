using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using CommunityToolkit.Maui.Core.Views;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using LightList.Services;
using LightList.Utils;

namespace LightList.ViewModels;

public partial class BaseTasksViewModel : ObservableObject
{
    private readonly ILogger _logger;
    protected ITaskViewModelFactory TaskViewModelFactory { get; }
    protected ITasksService TasksService { get; }
    protected IMessenger Messenger { get; }
    [ObservableProperty] private ObservableCollection<TaskViewModel> _allTasks = new();
    [ObservableProperty] private ObservableCollection<string?> _labels = new();
    [ObservableProperty] private ObservableCollection<DateOnly?> _dueDates = new();
    
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
    
    protected async Task GetTasks()
    {
        _logger.Debug($"Retrieving tasks");
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

    protected async Task GetLabels()
    {
        _logger.Debug($"Retrieving labels");

        try
        {
            var labels = await TasksService.GetLabels();

            Labels = new ObservableCollection<string?>(labels.Select(n => n.Name));

            Labels.Insert(0, null); // allow filter cancellation

            _logger.Debug($"Retrieved {Labels.Count} labels");
        }
        catch (Exception ex)
        {
            _logger.Error($"Failed to get labels: {ex.GetType()} - {ex.Message}");
            throw; // TODO: await DisplayAlert("Error retrieving labels. Please try again", ex.Message, "OK");
        }
        
    }
    
    partial void OnAllTasksChanged(ObservableCollection<TaskViewModel>? oldValue, ObservableCollection<TaskViewModel> newValue)
    {
        _logger.Debug($"All tasks changed");
        
        if (oldValue != null)
            oldValue.CollectionChanged -= AllTasks_CollectionChanged;

        newValue.CollectionChanged += AllTasks_CollectionChanged;
    }

    protected virtual void AllTasks_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        // Do nothing - derived class to override
    }
}

