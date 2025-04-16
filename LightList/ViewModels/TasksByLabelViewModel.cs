using System.Collections.ObjectModel;
using System.Collections.Specialized;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using LightList.Messages;
using LightList.Services;
using LightList.Utils;
using Label = LightList.Models.Label;

namespace LightList.ViewModels;

public partial class TasksByLabelViewModel : BaseTasksViewModel
{
    private readonly ILogger _logger;
    private bool _hasInitialized; // prevent retrieving data everytime page is navigated to
    [ObservableProperty] private ObservableCollection<string?> _labels = new();
    [ObservableProperty] private string? _selectedLabel;
    [ObservableProperty] private ObservableCollection<TaskViewModel> _tasksFiltered = new();

    public TasksByLabelViewModel(
        ITaskViewModelFactory taskViewModelFactory,
        ITasksService tasksService,
        IMessenger messenger,
        ILogger logger) : base(taskViewModelFactory, tasksService, messenger, logger)
    {
        _logger = logger;
        Messenger.Register<LabelsSyncedMessage>(this, async (recipient, _) => { await GetLabels(); });
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

    private async Task GetLabels()
    {
        _logger.Debug("Retrieving labels");

        try
        {
            List<Label> labels = await TasksService.GetLabels();

            var labelNames = new ObservableCollection<string?>(labels.Select(n => n.Name));

            _logger.Debug($"Retrieved {labelNames.Count} labels");

            labelNames.Insert(0, null); // allow filter cancellation

            Labels = labelNames; // do last to only trigger OnLabelsChanged after adding null
        }
        catch (Exception ex)
        {
            _logger.Error($"Failed to get labels: {ex.GetType()} - {ex.Message}");
            throw; // TODO: await DisplayAlert("Error retrieving labels. Please try again", ex.Message, "OK");
        }
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