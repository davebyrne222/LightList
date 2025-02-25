using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using System.Windows.Input;
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

    public int NoDaysRemaining => _task.DueDate.Subtract(DateTime.Today).Days;

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
    
    public TaskViewModel(ITasksService tasksService, Models.Task task)
    {
        TasksService = tasksService;
        _task = task;
        SaveCommand = new AsyncRelayCommand(Save);
        DeleteCommand = new AsyncRelayCommand(Delete);
    }
    
    private async Task Save()
    {
        TasksService.SaveTask(_task);
        await Shell.Current.GoToAsync($"..?saved={_task.Id}");
    }

    private async Task Delete()
    {
        TasksService.DeleteTask(_task);
        await Shell.Current.GoToAsync($"..?deleted={_task.Id}");
    }
    
    // the query string parameters used in navigation are passed to the ApplyQueryAttributes method
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
