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

public class AllTasksViewModel: INotifyPropertyChanged
{
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
    private ITaskViewModelFactory TaskViewModelFactory { get; }
    private ITasksService TasksService { get; }
    
    public AllTasksViewModel(ITaskViewModelFactory taskViewModelFactory, ITasksService tasksService)
    {
        TaskViewModelFactory = taskViewModelFactory;
        TasksService = tasksService;
        AllTasks = new ObservableCollection<TaskViewModel>(TasksService.GetTasks().Select(n => TaskViewModelFactory.Create(n)));
    }
    
    public event PropertyChangedEventHandler PropertyChanged;
    protected virtual void OnPropertyChanged(string propertyName)
    {
        Debug.WriteLine($"----[AllTasksViewModel] PropertyChanged: {propertyName}]");
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}