using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.Messaging;
using LightList.Messages;
using LightList.Services;
using LightList.Utils;
using LightList.ViewModels;

namespace LightList.Views.Components;

public partial class TaskListView : ContentView
{
    private readonly ITaskViewModelFactory _taskViewModelFactory;
    private readonly ITasksService _tasksService;

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
        get => (ObservableCollection<TaskViewModel>)GetValue(TasksProperty);
        set => SetValue(TasksProperty, value);
    }
    
    public TaskListView() : this(
        MauiProgram.GetService<ITaskViewModelFactory>(),
        MauiProgram.GetService<ITasksService>(), 
        MauiProgram.GetService<IMessenger>()) { }
    
    public TaskListView(ITaskViewModelFactory taskViewModelFactory, ITasksService tasksService, IMessenger messenger)
    {
        Logger.Log("Initializing");
        
        InitializeComponent();
        _taskViewModelFactory = taskViewModelFactory;
        _tasksService = tasksService;

        messenger.Register<TaskSavedMessage>(this, async (recipient, message) =>
        {
            await OnTaskSaved(message.Value);
        });
        
        messenger.Register<TaskCompletedMessage>(this, (recipient, message) =>
        {
            OnTaskCompleted(message.Value);
        });

        messenger.Register<TaskDeletedMessage>(this, (recipient, message) =>
        {
            OnTaskDeleted(message.Value);
        });
    }
    
    private static void OnTasksChanged(BindableObject bindable, object oldValue, object newValue)
    {
        Logger.Log($"\nbindable={bindable.GetType()}\noldValue={oldValue}\nnewValue={newValue}");
    }
    
    private int GetInsertionIndex(DateTime dueDate)
    {
        TaskViewModel? insertBeforeTask = Tasks.FirstOrDefault(t => t.DueDate > dueDate && t.Complete == false);
        
        Logger.Log($"insert before: {insertBeforeTask?.Id}");
        
        if (insertBeforeTask == null)
            insertBeforeTask = Tasks.FirstOrDefault(t => t.Complete);
        
        Logger.Log($"insert before: {insertBeforeTask?.Id}");
        
        return insertBeforeTask == null ? Tasks.Count -1 : Tasks.IndexOf(insertBeforeTask);
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
    
    private void ScrollToTask(TaskViewModel task)
    {
        if (TasksCollection != null)
        {
            TasksCollection.ScrollTo(task, null, ScrollToPosition.Center, true);
        }
    }
    
    private async Task OnTaskSaved(int taskId)
    {
        Logger.Log($"taskId = {taskId}");
        TaskViewModel? task = Tasks.FirstOrDefault(n => n.Id == taskId);
        
        if (task != null)
        {
            await task.Reload();
            Tasks.Move(Tasks.IndexOf(task), GetInsertionIndex(task.DueDate));
            Logger.Log($"Updated taskId {task.Id} in tasks list");
        }

        else
        {
            task = _taskViewModelFactory.Create(await _tasksService.GetTask(taskId));
            Tasks.Insert(GetInsertionIndex(task.DueDate), task);
            Logger.Log($"Added task {task.Id} in tasks list");
        }
        
        ScrollToTask(task);
    }
    
    private async Task OnTaskCompleted(int taskId)
    {
        Logger.Log($"taskId = {taskId}");
        TaskViewModel? matchedTask = Tasks.FirstOrDefault(n => n.Id == taskId);
        
        if (matchedTask != null)
        {
            await matchedTask.Reload();
            Tasks.Move(Tasks.IndexOf(matchedTask), Tasks.Count - 1);
            Logger.Log($"Updated taskId {matchedTask.Id} in tasks list");
        }
    }

    private void OnTaskDeleted(int taskId)
    {
        Logger.Log($"taskId = {taskId}");
        TaskViewModel? matchedTask = this.Tasks.FirstOrDefault(n => n.Id == taskId);

        if (matchedTask != null)
            Tasks.Remove(matchedTask);
    }
}