using System.Collections.ObjectModel;
using System.Collections.Specialized;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using LightList.Services;
using LightList.Utils;

namespace LightList.ViewModels;

public partial class TasksByDueDateViewModel : BaseTasksViewModel
{
    private readonly ILogger _logger;
    [ObservableProperty] private ObservableCollection<string?> _dueDates = new();
    [ObservableProperty] private string? _selectedDate;
    [ObservableProperty] private ObservableCollection<TaskViewModel> _tasksFiltered = new();
    private bool _hasInitialized;

    public TasksByDueDateViewModel(
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
        await base.OnNavigatedTo();
        await GetDueDates();
        GetFilteredTasks();
        _hasInitialized = true;

    }
    
    protected async override void AllTasks_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        await GetDueDates();
        GetFilteredTasks();
    }

    partial void OnSelectedDateChanged(string? oldValue, string? newValue)
    {
        GetFilteredTasks();
    }
    
    private async Task GetDueDates()
    {
        _logger.Debug("Retrieving due dates");

        try
        {
            List<DateOnly> dueDates = await TasksService.GetDueDates();

            var dates = new ObservableCollection<string?>(dueDates.Select(n => n.ToShortDateString()));
            
            _logger.Debug($"Retrieved {dates.Count} due dates");
            
            dates.Insert(0, null); // allow filter cancellation
            
            DueDates = dates;
        }
        catch (Exception ex)
        {
            _logger.Error($"Failed to get due dates: {ex.GetType()} - {ex.Message}");
            throw; // TODO: await DisplayAlert("Error retrieving DueDates. Please try again", ex.Message, "OK");
        }

    }

    private void GetFilteredTasks()
    {
        _logger.Debug($"Getting filtered tasks: (date: {SelectedDate})");

        // Deselect filter
        if (SelectedDate == null)
        {
            TasksFiltered = AllTasks;
            return;
        }

        var filtered = AllTasks
            .Where(task => task.DueAt.ToShortDateString() == SelectedDate)
            .ToList();

        TasksFiltered = new ObservableCollection<TaskViewModel>(filtered);

        _logger.Debug($"Got {filtered.Count} tasks");
    }
}