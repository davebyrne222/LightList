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
    public DateTime Date => _task.Date;
    public string Identifier => _task.Filename; // TODO: replace Filename with ID (or similar)
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
        _task.Date = DateTime.Now;
        TasksService.SaveTask(_task);
        await Shell.Current.GoToAsync($"..?saved={_task.Filename}");
    }

    private async Task Delete()
    {
        TasksService.DeleteTask(_task);
        await Shell.Current.GoToAsync($"..?deleted={_task.Filename}");
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
        _task = TasksService.GetTask(_task.Filename);
        RefreshProperties();
    }

    private void RefreshProperties()
    {
        OnPropertyChanged(nameof(Text));
        OnPropertyChanged(nameof(Date));
    }
}
