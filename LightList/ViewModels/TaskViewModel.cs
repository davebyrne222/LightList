using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using LightList.Messages;
using LightList.Services;
using LightList.Utils;
using Task = LightList.Models.Task;

namespace LightList.ViewModels;

public class TaskViewModel : ObservableObject, IQueryAttributable
{
    private readonly ILogger _logger;
    private readonly LoggerContext _loggerContext;
    private readonly IMessenger _messenger;
    private readonly ITasksService _tasksService;
    private Task _task;

    public TaskViewModel(LoggerContext loggerContext, ILogger logger, ITasksService tasksService, IMessenger messenger,
        Task task)
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

    public string Label
    {
        get => _task.Label ?? string.Empty;
        set
        {
            if (_task.Label != value) _task.Label = value;
            OnPropertyChanged();
        }
    }

    public bool HasLabel => !string.IsNullOrEmpty(_task.Label);

    // [ObservableProperty] private bool _complete;
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
        _logger.Debug("Applying query attributes");
        if (query.ContainsKey("load"))
        {
            await LoadTaskAsync(query["load"].ToString()!);
            RefreshProperties();
        }
    }

    private async System.Threading.Tasks.Task LoadTaskAsync(string id)
    {
        _loggerContext.Group = "Load Task";
        _logger.Debug($"Loading task (id={id})");

        _task = await _tasksService.GetTask(id);

        _loggerContext.Reset();
    }

    private async System.Threading.Tasks.Task SaveTaskAsync()
    {
        _loggerContext.Group = "Save Task";

        _logger.Debug($"Saving task (id={_task.Id})");
        await _tasksService.SaveTask(_task);

        _logger.Debug($"Saved task. Sending Message (id={_task.Id})");
        _messenger.Send(new TaskSavedMessage(_task.Id));

        await Shell.Current.GoToAsync("..");

        _loggerContext.Reset();
    }

    private async System.Threading.Tasks.Task CompleteTaskAsync()
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

    private async System.Threading.Tasks.Task DeleteTaskAsync()
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

    public async System.Threading.Tasks.Task Reload()
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
}