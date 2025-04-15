using System.Collections.ObjectModel;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using LightList.Messages;
using LightList.Services;
using LightList.Utils;
using Task = System.Threading.Tasks.Task;

namespace LightList.ViewModels;

public partial class TaskViewModel : ObservableObject, IQueryAttributable
{
    private readonly ILogger _logger;
    private readonly LoggerContext _loggerContext;
    private readonly IMessenger _messenger;
    private readonly ITasksService _tasksService;
    [ObservableProperty] private ObservableCollection<string?> _labels = new();
    [ObservableProperty] private string? _selectedLabel;
    private Models.Task _task;

    public TaskViewModel(
        LoggerContext loggerContext,
        ILogger logger,
        ITasksService tasksService,
        IMessenger messenger,
        Models.Task task)
    {
        _loggerContext = loggerContext;
        _logger = logger;

        _logger.Debug("Initializing");

        _tasksService = tasksService;
        _messenger = messenger;
        _task = task;
        SaveCommand = new AsyncRelayCommand(SaveTaskAsync);
        CompleteCommand = new AsyncRelayCommand(CompleteTaskAsync);
        DeleteCommand = new AsyncRelayCommand(DeleteTaskAsync);
    }

    public string Id => _task.Id;

    public string Text
    {
        get => _task.Text;
        set
        {
            if (_task.Text != value)
            {
                _task.Text = value;
                OnPropertyChanged();
            }
        }
    }

    public DateTime DueDate
    {
        get => _task.DueAt;
        set
        {
            if (_task.DueAt != value)
            {
                _task.DueAt = value;
                OnPropertyChanged();
            }
        }
    }

    public int NoDaysRemaining => DueDate.Subtract(DateTime.Today).Days;

    public string NoDaysRemainingLbl
    {
        get
        {
            if (Complete)
                return "Done";

            switch (NoDaysRemaining)
            {
                case < 0: return "Overdue";
                case 0: return "Today";
                case 1: return "Tomorrow";
                default: return $"{NoDaysRemaining} Days";
            }
        }
    }

    public string? Label => _task.Label;

    public bool Complete
    {
        get => _task.IsCompleted;
        set
        {
            if (_task.IsCompleted != value)
            {
                _task.IsCompleted = value;
                _logger.Debug($"Task completed: {value}");
                OnPropertyChanged();
            }
        }
    }

    public ICommand SaveCommand { get; private set; }
    public ICommand CompleteCommand { get; private set; }
    public ICommand DeleteCommand { get; private set; }

    async void IQueryAttributable.ApplyQueryAttributes(IDictionary<string, object> query)
    {
        _loggerContext.Group = "Page Load";
        _logger.Debug("Applying query attributes");

        await LoadLabelsAsync();

        if (query.TryGetValue("load", out var value))
        {
            _logger.Debug("Loading task");
            await LoadTaskAsync(value.ToString()!);
        }

        _loggerContext.Reset();
    }

    private async Task LoadTaskAsync(string id)
    {
        _logger.Debug($"Loading task (id={id})");

        try
        {
            _task = await _tasksService.GetTask(id);
            SelectedLabel = _task.Label;
            RefreshProperties();
        }
        catch (Exception ex)
        {
            _logger.Error($"Failed to load task: {ex.GetType()} - {ex.Message}");
            throw; // TODO: show alert
        }
    }

    private async Task LoadLabelsAsync()
    {
        _logger.Debug("Retrieving labels");

        try
        {
            var labels = await _tasksService.GetLabels();
            Labels = new ObservableCollection<string?>(labels.Select(n => n.Name));
            _logger.Debug($"Retrieved {Labels.Count} labels");
            Labels.Insert(0, null); // allow de-select
        }
        catch (Exception ex)
        {
            _logger.Error($"Failed to get labels: {ex.GetType()} - {ex.Message}");
            throw; // TODO: await DisplayAlert("Error retrieving labels. Please try again", ex.Message, "OK");
        }
    }

    public async Task AddLabelAsync(string label)
    {
        _logger.Debug($"Adding label (label={label})");

        try
        {
            // Save label
            Models.Label model = new();
            model.Name = label;
            await _tasksService.SaveLabel(model);

            // Update UI
            await LoadLabelsAsync();
            SelectedLabel = label;
        }
        catch (Exception ex)
        {
            _logger.Error($"Failed to save label: {ex.GetType()} - {ex.Message}");
            throw; // TODO: show alert
        }
    }

    private async Task SaveTaskAsync()
    {
        _loggerContext.Group = "Save Task";

        _logger.Debug($"Saving task (id={_task.Id})");

        await _tasksService.SaveTask(_task);

        _logger.Debug($"Saved task. Sending Message (id={_task.Id})");
        _messenger.Send(new TaskSavedMessage(_task.Id));

        await Shell.Current.GoToAsync("..");

        _loggerContext.Reset();
    }

    private async Task CompleteTaskAsync()
    {
        _loggerContext.Group = "Complete Task";

        _logger.Debug($"Completing task id={_task.Id}");
        Complete = true;
        await _tasksService.SaveTask(_task);

        _logger.Debug($"Saved task (id={_task.Id})");
        _messenger.Send(new TaskCompletedMessage(_task.Id));

        await Shell.Current.GoToAsync("..");

        _loggerContext.Reset();
    }

    private async Task DeleteTaskAsync()
    {
        _loggerContext.Group = "Delete Task";

        _logger.Debug($"Deleting task id={_task.Id}");
        await _tasksService.DeleteTask(_task);

        _logger.Debug("Sending deleted message");
        _messenger.Send(new TaskDeletedMessage(_task.Id));

        _logger.Debug("Navigating to previous page");
        await Shell.Current.GoToAsync("..");

        _loggerContext.Reset();
    }

    public async Task Reload()
    {
        _loggerContext.Group = "Reload Task";

        _logger.Debug($"Reloading task id={_task.Id}");
        await LoadTaskAsync(_task.Id);
        RefreshProperties();

        _loggerContext.Reset();
    }

    private void RefreshProperties()
    {
        _logger.Debug("Refreshing properties");
        OnPropertyChanged(nameof(Text));
        OnPropertyChanged(nameof(DueDate));
        OnPropertyChanged(nameof(NoDaysRemaining));
        OnPropertyChanged(nameof(NoDaysRemainingLbl));
        OnPropertyChanged(nameof(Label));
    }

    partial void OnSelectedLabelChanged(string? oldValue, string? newValue)
    {
        _task.Label = SelectedLabel;
    }
}