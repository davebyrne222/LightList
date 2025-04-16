using System.Collections.ObjectModel;
using System.Collections.Specialized;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using LightList.Services;
using LightList.Utils;

namespace LightList.ViewModels;

public partial class TasksByLabelViewModel : BaseTasksViewModel
{
    private readonly ILogger _logger;
    private bool _hasInitialized; // prevent retrieving data everytime page is navigated to
    [ObservableProperty] private string? _selectedLabel;
    [ObservableProperty] private ObservableCollection<TaskViewModel> _tasksFiltered = new();

    public TasksByLabelViewModel(
        ITaskViewModelFactory taskViewModelFactory,
        ITasksService tasksService,
        IMessenger messenger,
        ILogger logger) : base(taskViewModelFactory, tasksService, messenger, logger)
    {
        _logger = logger;
    }

    public new async Task OnNavigatedTo()
    {
        if (_hasInitialized)
            return;

        _logger.Debug("OnNavigatedTo");
        await GetTasks();
        await GetLabels();
        GetFilteredTasks();
        _hasInitialized = true;
    }

    protected override void AllTasks_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        GetFilteredTasks();
    }

    partial void OnSelectedLabelChanged(string? oldValue, string? newValue)
    {
        GetFilteredTasks();
    }

    private void GetFilteredTasks()
    {
        _logger.Debug($"Getting filtered tasks: (label: {SelectedLabel})");

        // Deselect filter
        if (SelectedLabel == null)
        {
            TasksFiltered = AllTasks;
            return;
        }

        var filtered = AllTasks
            .Where(task => task.Label != null && task.Label.Contains(SelectedLabel))
            .ToList();

        TasksFiltered = new ObservableCollection<TaskViewModel>(filtered);

        _logger.Debug($"Got {filtered.Count} tasks");
    }
}