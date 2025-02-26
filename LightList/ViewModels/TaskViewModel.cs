using System.Diagnostics;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using System.Windows.Input;
using CommunityToolkit.Mvvm.Messaging;
using LightList.Messages;
using LightList.Services;

namespace LightList.ViewModels;

public class TaskViewModel: ObservableObject, IQueryAttributable
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
    public string NoDaysRemainingLbl => GetDaysRemainingLabel(_task.DueDate.Subtract(DateTime.Today).Days);
    public string Label
    {
        get => _task.Label;
        set
        {
            if (_task.Label != value)
            {
                _task.Label = value;
                OnPropertyChanged();
            }
        }
    }
    public string Id => _task.Id;
    public ICommand SaveCommand { get; private set; }
    public ICommand DeleteCommand { get; private set; }
    private ITasksService TasksService { get; }
    
    private readonly IMessenger _messenger;
    
    public TaskViewModel(ITasksService tasksService, IMessenger messenger, Models.Task task)
    {
        TasksService = tasksService;
        _messenger = messenger;
        _task = task;
        SaveCommand = new AsyncRelayCommand(SaveTaskAsync);
        DeleteCommand = new AsyncRelayCommand(DeleteTaskAsync);
    }
    
    private async Task SaveTaskAsync()
    {
        TasksService.SaveTask(_task);
        _messenger.Send(new TaskSavedMessage(_task.Id));
        
        Debug.WriteLine($"Task saved: {_task.Id}");

        await Shell.Current.GoToAsync($"..");
    }

    private async Task DeleteTaskAsync()
    {
        TasksService.DeleteTask(_task);
        _messenger.Send(new TaskDeletedMessage(_task.Id));
        
        Debug.WriteLine($"Task Deleted: {_task.Id}");
        
        await Shell.Current.GoToAsync($"..");
    }
    
    private string GetDaysRemainingLabel(int noDaysRemaining)
    {
        switch(noDaysRemaining) {
            case < 0: return "Overdue";
            case 0: return "Today";
            case 1: return $"{noDaysRemaining} Day";
            default: return $"{noDaysRemaining} Days";
        }   
    }
    
    void IQueryAttributable.ApplyQueryAttributes(IDictionary<string, object> query)
    {
        if (query.ContainsKey("load"))
        {
            _task = TasksService.GetTask(query["load"].ToString());
            RefreshProperties();
        }
    }
    
    public void Reload()
    {
        _task = TasksService.GetTask(_task.Id);
        RefreshProperties();
    }

    private void RefreshProperties()
    {
        OnPropertyChanged(nameof(Text));
        OnPropertyChanged(nameof(DueDate));
        OnPropertyChanged(nameof(Label));
    }
}
