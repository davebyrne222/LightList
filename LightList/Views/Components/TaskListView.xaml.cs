using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.Messaging;
using LightList.Messages;
using LightList.Services;
using LightList.Utils;
using LightList.ViewModels;

namespace LightList.Views.Components;

public partial class TaskListView : ContentView
{
    #region Fields
    
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
    
    #endregion
    
    #region Ctor
    
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
        
        messenger.Register<TaskCompletedMessage>(this, async (recipient, message) =>
        {
            await OnTaskCompleted(message.Value);
        });

        messenger.Register<TaskDeletedMessage>(this, (recipient, message) =>
        {
            OnTaskDeleted(message.Value);
        });
    }
    
    #endregion
    
    #region Event Handlers
    
    // TODO: remove? Seems redundant
    private static void OnTasksChanged(BindableObject bindable, object oldValue, object newValue)
    {
        Logger.Log($"\nbindable={bindable.GetType()}\noldValue={oldValue}\nnewValue={newValue}");
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
    
    /// <summary>
    /// Event handler for when a task is added or updated.
    ///
    /// If new, create new TaskViewModel object and add to Tasks ObservableCollection.
    /// If updated, reload task and update position in Tasks ObservableCollection
    /// </summary>
    /// <param name="taskId">Id of task that was created or updated</param>
    private async Task OnTaskSaved(int taskId)
    {
        Logger.Log($"taskId {taskId} saved. Updating task list");
        TaskViewModel? task = Tasks.FirstOrDefault(n => n.Id == taskId);

        // Task is new
        if (task == null)
        {
            task = _taskViewModelFactory.Create(await _tasksService.GetTask(taskId));
            Tasks.Insert(GetInsertionIndex(task.DueDate), task);
            Logger.Log($"New task added (id={task.Id}) to index {Tasks.IndexOf(task)}");
        }
        // Task was updated
        else
        {
            await task.Reload();
            int idx = GetInsertionIndex(task.DueDate) - 1;
            Tasks.Move(Tasks.IndexOf(task), idx < 0 ? 0 : idx);
            Logger.Log($"Updated taskId {task.Id} in tasks list to index {Tasks.IndexOf(task)}");
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
    
    #endregion

    #region Utils
    
    /// <summary>
    /// Determine the index in the Tasks ObservableCollection that a new or updated task should be inserted/moved to
    ///
    /// Insertion preference:
    /// 1. before incomplete task with later due date
    /// 2. before complete tasks
    /// 3. end of Tasks ObservableCollection
    /// </summary>
    /// <param name="dueDate">Due date of new/updated task</param>
    /// <returns></returns>
    private int GetInsertionIndex(DateTime dueDate)
    {
        Logger.Log($"Inserting task");
        
        // Insert before incomplete task with the next due date but
        TaskViewModel? insertBeforeTask = Tasks.FirstOrDefault(t => t.DueDate > dueDate && t.Complete == false);
        
        // If no incomplete tasks with later due date, add before completed tasks
        if (insertBeforeTask == null)
        {
            Logger.Log("No incomplete task with greater due date found. Searching for complete tasks");
            insertBeforeTask = Tasks.FirstOrDefault(t => t.Complete);
        }
        
        // if no complete tasks, insert at end
        if (insertBeforeTask == null)
            Logger.Log($"No complete tasks found. Inserting at end (idx={Tasks.Count})");
        else
            Logger.Log($"Inserting before task id {insertBeforeTask.Id} with index {Tasks.IndexOf(insertBeforeTask)}");
        
        return insertBeforeTask == null ? Tasks.Count : Tasks.IndexOf(insertBeforeTask);
    }
    
    /// <summary>
    /// Ensures new or updated task is visible in the view
    /// </summary>
    /// <param name="task">Task to be scrolled into view</param>
    private void ScrollToTask(TaskViewModel task)
    {
        Logger.Log($"Scrolling taskId {task.Id} into view");
        
        if (TasksCollection != null)
        {
            TasksCollection.ScrollTo(task, null, ScrollToPosition.Center, true);
        }
    }
    
    #endregion
}