using System.Diagnostics;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using System.Windows.Input;
using CommunityToolkit.Mvvm.Messaging;
using LightList.Messages;
using LightList.Services;
using LightList.Utils;

namespace LightList.ViewModels;

public partial class TaskViewModel: ObservableObject, IQueryAttributable
{
    private Models.Task _task;
    public string Text
    {
        get => _task.Text;
        set
        {
            if (_task.Text != value)
            {
                _task.Text = value;
                Logger.Log($"Text changed: {value}");
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
                Logger.Log($"DueDate changed: {value}");
                OnPropertyChanged();
            }
        }
    }

    // [ObservableProperty] private DateTime _dueDate;
    public int NoDaysRemaining => DueDate.Subtract(DateTime.Today).Days;
    public string NoDaysRemainingLbl
    {
        get
        {
            switch(NoDaysRemaining) {
                case < 0: return "Overdue";
                case 0: return "Today";
                case 1: return $"{NoDaysRemaining} Day";
                default: return $"{NoDaysRemaining} Days";
            } 
        }
    }
    public string Label
    {
        get => _task.Label;
        set
        {
            if (_task.Label != value)
            {
                _task.Label = value;
                Logger.Log($"Label changed: {value}");
                OnPropertyChanged();
            }
        }
    }
    public bool HasLabel => !String.IsNullOrEmpty(_task.Label);
    public int Id => _task.Id;
    public ICommand SaveCommand { get; private set; }
    public ICommand DeleteCommand { get; private set; }
    private ITasksService TasksService { get; }
    private readonly IMessenger _messenger;
    
    public TaskViewModel(ITasksService tasksService, IMessenger messenger, Models.Task task)
    {
        Logger.Log("Initializing");
        TasksService = tasksService;
        _messenger = messenger;
        _task = task;
        SaveCommand = new AsyncRelayCommand(SaveTaskAsync);
        DeleteCommand = new AsyncRelayCommand(DeleteTaskAsync);
    }
    
    private async Task SaveTaskAsync()
    {
        Logger.Log($"Saving task (id={_task.Id})");
        await TasksService.SaveTask(_task);

        Logger.Log($"Saved task (id={_task.Id})");
        _messenger.Send(new TaskSavedMessage(_task.Id));
        
        await Shell.Current.GoToAsync($"..");
    }

    private async Task DeleteTaskAsync()
    {
        Logger.Log($"Deleted task (id={_task.Id})");
        TasksService.DeleteTask(_task);
        
        Logger.Log($"Sending deleted message");
        _messenger.Send(new TaskDeletedMessage(_task.Id));

        Logger.Log($"Navigating to previous page");
        await Shell.Current.GoToAsync($"..");
    }

    private async Task LoadTaskAsync(int id)
    {
        Logger.Log($"Loading task (id={id})");
        _task = await TasksService.GetTask(id);
    }
    
    async void IQueryAttributable.ApplyQueryAttributes(IDictionary<string, object> query)
    {
        Logger.Log($"Applying query attributes");
        if (query.ContainsKey("load"))
        {
            await LoadTaskAsync(Convert.ToInt32(query["load"]));
            RefreshProperties();
        }
    }
    
    public async Task Reload()
    {
        Logger.Log($"Reloading task (id={_task.Id})");
        await LoadTaskAsync(_task.Id);
        RefreshProperties();
    }

    private void RefreshProperties()
    {
        Logger.Log($"Refreshing properties");
        OnPropertyChanged(nameof(Text));
        OnPropertyChanged(nameof(DueDate));
        OnPropertyChanged(nameof(NoDaysRemaining));
        OnPropertyChanged(nameof(NoDaysRemainingLbl));
        OnPropertyChanged(nameof(Label));
    }
}
