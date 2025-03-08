using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows.Input;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using LightList.Messages;
using LightList.Services;
using LightList.Utils;
using LightList.ViewModels;

namespace LightList.Views.Components;

public partial class TaskListView : ContentView
{
    private ITaskViewModelFactory TaskViewModelFactory { get; }
    private ITasksService TasksService { get; }

    public static readonly BindableProperty TasksProperty =
        BindableProperty.Create(
            nameof(Tasks), 
            typeof(ObservableCollection<TaskViewModel>), 
            typeof(TaskListView),
            defaultValue: new ObservableCollection<TaskViewModel>(),
            defaultBindingMode: BindingMode.TwoWay,
            propertyChanged: OnTasksChanged);

    public ObservableCollection<TaskViewModel> Tasks
    {
        get => (ObservableCollection<TaskViewModel>) GetValue(TasksProperty);
        set => SetValue(TasksProperty, value);
    }
    
    public TaskListView() : this(
        MauiProgram.GetService<ITaskViewModelFactory>(),
        MauiProgram.GetService<ITasksService>(), 
        MauiProgram.GetService<IMessenger>()) { }
    
    public TaskListView(ITaskViewModelFactory taskViewModelFactory, ITasksService tasksService, IMessenger messenger)
    {
        InitializeComponent();
        TaskViewModelFactory = taskViewModelFactory;
        TasksService = tasksService;

        messenger.Register<TaskSavedMessage>(this, async (recipient, message) =>
        {
            await OnTaskSaved(message.Value);
        });

        messenger.Register<TaskDeletedMessage>(this, (recipient, message) =>
        {
            OnTaskDeleted(message.Value);
        });
    }
    
    private static void OnTasksChanged(BindableObject bindable, object oldValue, object newValue)
    {
        Logger.Log($"bindable={bindable.GetType()} oldValue={oldValue} newValue={newValue}");
    }
    
    private async void OnTaskSelected(object sender, SelectionChangedEventArgs e)
    {
        Logger.Log($"Selection count = {e.CurrentSelection.Count}");

        if (e.CurrentSelection.Count == 0)
            return;

        var selectedTask = e.CurrentSelection[0] as TaskViewModel; 

        Logger.Log($"Selected task id = {selectedTask.Id}");

        if (selectedTask != null)
            await Shell.Current.GoToAsync($"{nameof(Views.TaskPage)}?load={selectedTask.Id}");

        // Deselect the item
        ((CollectionView)sender).SelectedItem = null;
    }

    void OnTaskDeleted(int taskId)
    {
        TaskViewModel? matchedTask = this.Tasks.FirstOrDefault(n => n.Id == taskId);

        if (matchedTask != null)
            Tasks.Remove(matchedTask);
    }

    async Task OnTaskSaved(int taskId)
    {
        Logger.Log($"taskId = {taskId}");
        TaskViewModel? matchedTask = Tasks.FirstOrDefault(n => n.Id == taskId);
        
        if (matchedTask != null)
        {
            await matchedTask.Reload();
            Tasks.Move(Tasks.IndexOf(matchedTask), 0);
        }

        else
        {
            var task = await TasksService.GetTask(taskId);
            Tasks.Insert(0, TaskViewModelFactory.Create(task));
        }
    }
}