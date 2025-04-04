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
    private readonly IMessenger _messenger;
    private readonly ITasksService _tasksService;
    private Task _task;

    public TaskViewModel(ITasksService tasksService, IMessenger messenger, Task task)
    {
        Logger.Log("Initializing");
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
        get => _task.DueDate;
        set
        {
            if (_task.DueDate != value)
            {
                _task.DueDate = value;
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
        get => _task.Complete;
        set
        {
            if (_task.Complete != value)
            {
                _task.Complete = value;
                Logger.Log($"Complete changed: {value}");
                OnPropertyChanged();
            }
        }
    }

    public ICommand SaveCommand { get; private set; }
    public ICommand CompleteCommand { get; private set; }
    public ICommand DeleteCommand { get; private set; }

    async void IQueryAttributable.ApplyQueryAttributes(IDictionary<string, object> query)
    {
        Logger.Log("Applying query attributes");
        if (query.ContainsKey("load"))
        {
            await LoadTaskAsync(query["load"].ToString()!);
            RefreshProperties();
        }
    }

    private async System.Threading.Tasks.Task LoadTaskAsync(string id)
    {
        Logger.Log($"Loading task (id={id})");
        _task = await _tasksService.GetTask(id);
    }

    private async System.Threading.Tasks.Task SaveTaskAsync()
    {
        Logger.Log($"Saving task (id={_task.Id})");
        await _tasksService.SaveTask(_task);

        Logger.Log($"Saved task. Sending Message (id={_task.Id})");
        _messenger.Send(new TaskSavedMessage(_task.Id));

        await Shell.Current.GoToAsync("..");
    }

    private async System.Threading.Tasks.Task CompleteTaskAsync()
    {
        Logger.Log($"Completing task id={_task.Id}");
        Complete = true;
        await _tasksService.SaveTask(_task);

        Logger.Log($"Saved task (id={_task.Id})");
        _messenger.Send(new TaskCompletedMessage(_task.Id));

        await Shell.Current.GoToAsync("..");
    }

    private async System.Threading.Tasks.Task DeleteTaskAsync()
    {
        Logger.Log($"Deleted task id={_task.Id}");
        _tasksService.DeleteTask(_task);

        Logger.Log("Sending deleted message");
        _messenger.Send(new TaskDeletedMessage(_task.Id));

        Logger.Log("Navigating to previous page");
        await Shell.Current.GoToAsync("..");
    }

    public async System.Threading.Tasks.Task Reload()
    {
        Logger.Log($"Reloading task id={_task.Id}");
        await LoadTaskAsync(_task.Id);
        RefreshProperties();
    }

    private void RefreshProperties()
    {
        Logger.Log("Refreshing properties");
        OnPropertyChanged(nameof(Text));
        OnPropertyChanged(nameof(DueDate));
        OnPropertyChanged(nameof(NoDaysRemaining));
        OnPropertyChanged(nameof(NoDaysRemainingLbl));
        OnPropertyChanged(nameof(Label));
    }
}