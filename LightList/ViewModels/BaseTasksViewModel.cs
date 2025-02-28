using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows.Input;
using CommunityToolkit.Mvvm.Messaging;
using LightList.Messages;
using LightList.Services;
using LightList.Views;
using LightList.Views.Components;

namespace LightList.ViewModels;

public class BaseTasksViewModel: INotifyPropertyChanged
{
    protected ITaskViewModelFactory TaskViewModelFactory { get; }
    protected ITasksService TasksService { get; }
    
    private ObservableCollection<TaskViewModel> _tasks;
    public ObservableCollection<TaskViewModel> AllTasks
    {
        get => _tasks;
        set
        {
            if (_tasks != value)
            {
                _tasks = value;
                OnPropertyChanged(nameof(AllTasks));
            }
        }
    }

    public BaseTasksViewModel(ITaskViewModelFactory taskViewModelFactory, ITasksService tasksService)
    {
        TaskViewModelFactory = taskViewModelFactory;
        TasksService = tasksService;
    }
    
    public event PropertyChangedEventHandler PropertyChanged;
    protected virtual void OnPropertyChanged(string propertyName)
    {
        Debug.WriteLine($"----[AllTasksViewModel] PropertyChanged: {propertyName}]");
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}