using System.Collections.ObjectModel;
using System.Collections.Specialized;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using LightList.Messages;
using LightList.Services;
using LightList.Utils;

namespace LightList.ViewModels;

public partial class TasksByLabelViewModel : BaseTasksViewModel
{
    private readonly ILogger _logger;

    [ObservableProperty] private ObservableCollection<TaskViewModel> _tasksFiltered = new();
    [ObservableProperty] private string? _selectedLabel;

    public TasksByLabelViewModel(
        ITaskViewModelFactory taskViewModelFactory,
        ITasksService tasksService,
        IMessenger messenger,
        ILogger logger) : base(taskViewModelFactory, tasksService, messenger, logger)
    {
        _logger = logger;
        Messenger.Register<TasksSyncedMessage>(this, async (recipient, _) => { await GetTasks(); });
    }
    
    public async Task OnAppearing()
    {
        _logger.Debug($"OnAppearing");
        await base.GetTasks();
        await base.GetLabels();
        GetFilteredTasks();
    }
    
    private void GetFilteredTasks()
    {
        _logger.Debug("Getting filtered tasks");

        // Deselect filter
        if (SelectedLabel == null)
        {
            TasksFiltered = AllTasks;
            return;
        }
        
        var filtered = AllTasks
            .Where(task => task.SelectedLabel != null && task.SelectedLabel.Contains(SelectedLabel))
            .ToList();

        TasksFiltered = new ObservableCollection<TaskViewModel>(filtered);
        
        _logger.Debug($"Got {filtered.Count} tasks");
    }

    protected override void AllTasks_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e) => GetFilteredTasks();

    partial void OnSelectedLabelChanged(string? oldValue, string? newValue) => GetFilteredTasks();
}