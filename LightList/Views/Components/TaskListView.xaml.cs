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

    private readonly ILogger _logger;
    private readonly ITaskViewModelFactory _taskViewModelFactory;
    private readonly ITasksService _tasksService;

    public static readonly BindableProperty TasksProperty =
        BindableProperty.Create(
            nameof(Tasks),
            typeof(ObservableCollection<TaskViewModel>),
            typeof(TaskListView),
            new ObservableCollection<TaskViewModel>(),
            BindingMode.TwoWay);

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
        MauiProgram.GetService<IMessenger>(),
        MauiProgram.GetService<ILogger>())
    {
    }

    public TaskListView(ITaskViewModelFactory taskViewModelFactory, ITasksService tasksService, IMessenger messenger,
        ILogger logger)
    {
        _logger = logger;
        _logger.Debug("Initializing");

        InitializeComponent();
        _taskViewModelFactory = taskViewModelFactory;
        _tasksService = tasksService;

        messenger.Register<TaskSavedMessage>(this, async (recipient, message) => { await OnTaskSaved(message.Value); });

        messenger.Register<TaskCompletedMessage>(this,
            async (recipient, message) => { await OnTaskCompleted(message.Value); });

        messenger.Register<TaskDeletedMessage>(this, (recipient, message) => { OnTaskDeleted(message.Value); });
    }

    #endregion

    #region Event Handlers

    // TODO: remove
    private static void OnTasksChanged(BindableObject bindable, object oldValue, object newValue)
    {
        Console.WriteLine($"Bindable changed: {bindable.GetType()}");
    }

    private async void OnTaskSelected(object sender, SelectionChangedEventArgs e)
    {
        _logger.Debug($"Selection count = {e.CurrentSelection.Count}");

        if (e.CurrentSelection.Count == 0)
            return;

        var selectedTask = e.CurrentSelection[0] as TaskViewModel;

        _logger.Debug($"Selected task id = {selectedTask.Id}");

        if (selectedTask != null)
            await Shell.Current.GoToAsync($"{nameof(TaskPage)}?load={selectedTask.Id}");

        // Deselect the item
        ((CollectionView)sender).SelectedItem = null;
    }

    /// <summary>
    ///     Event handler for when a task is added or updated.
    ///     If new, create new TaskViewModel object and add to Tasks ObservableCollection.
    ///     If updated, reload task and update position in Tasks ObservableCollection
    /// </summary>
    /// <param name="taskId">Id of task that was created or updated</param>
    private async Task OnTaskSaved(string taskId)
    {
        _logger.Debug($"taskId {taskId} saved. Updating task list");
        var task = Tasks.FirstOrDefault(n => n.Id == taskId);

        // Task is new
        if (task == null)
        {
            task = _taskViewModelFactory.Create(await _tasksService.GetTask(taskId));
            Tasks.Insert(GetInsertionIndex(task.DueDate), task);
            _logger.Debug($"New task added (id={task.Id}) to index {Tasks.IndexOf(task)}");
        }
        // Task was updated
        else
        {
            await task.Reload();
            var idx = GetInsertionIndex(task.DueDate) - 1;
            Tasks.Move(Tasks.IndexOf(task), idx < 0 ? 0 : idx);
            _logger.Debug($"Updated taskId {task.Id} in tasks list to index {Tasks.IndexOf(task)}");
        }

        ScrollToTask(task);
    }

    private async Task OnTaskCompleted(string taskId)
    {
        _logger.Debug($"taskId = {taskId}");
        var matchedTask = Tasks.FirstOrDefault(n => n.Id == taskId);

        if (matchedTask != null)
        {
            await matchedTask.Reload();
            Tasks.Move(Tasks.IndexOf(matchedTask), Tasks.Count - 1);
            _logger.Debug($"Updated taskId {matchedTask.Id} in tasks list");
        }
    }

    private void OnTaskDeleted(string taskId)
    {
        _logger.Debug($"taskId = {taskId}");
        var matchedTask = Tasks.FirstOrDefault(n => n.Id == taskId);

        if (matchedTask != null)
            Tasks.Remove(matchedTask);
    }

    #endregion

    #region Utils

    /// <summary>
    ///     Determine the index in the Tasks ObservableCollection that a new or updated task should be inserted/moved to
    ///     Insertion preference:
    ///     1. before incomplete task with later due date
    ///     2. before complete tasks
    ///     3. end of Tasks ObservableCollection
    /// </summary>
    /// <param name="dueDate">Due date of new/updated task</param>
    /// <returns></returns>
    private int GetInsertionIndex(DateTime dueDate)
    {
        _logger.Debug("Inserting task");

        // Insert before incomplete task with the next due date but
        var insertBeforeTask = Tasks.FirstOrDefault(t => t.DueDate > dueDate && t.Complete == false);

        // If no incomplete tasks with later due date, add before completed tasks
        if (insertBeforeTask == null)
        {
            _logger.Debug("No incomplete task with greater due date found. Searching for complete tasks");
            insertBeforeTask = Tasks.FirstOrDefault(t => t.Complete);
        }

        // if no complete tasks, insert at end
        if (insertBeforeTask == null)
            _logger.Debug($"No complete tasks found. Inserting at end (idx={Tasks.Count})");
        else
            _logger.Debug(
                $"Inserting before task id {insertBeforeTask.Id} with index {Tasks.IndexOf(insertBeforeTask)}");

        return insertBeforeTask == null ? Tasks.Count : Tasks.IndexOf(insertBeforeTask);
    }

    /// <summary>
    ///     Ensures new or updated task is visible in the view
    /// </summary>
    /// <param name="task">Task to be scrolled into view</param>
    private void ScrollToTask(TaskViewModel task)
    {
        _logger.Debug($"Scrolling taskId {task.Id} into view");

        if (TasksCollection != null) TasksCollection.ScrollTo(task, null, ScrollToPosition.Center);
    }

    #endregion
}