using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Windows.Input;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using LightList.Messages;
using LightList.Services;
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

        messenger.Register<TaskSavedMessage>(this, (recipient, message) =>
        {
            OnTaskSaved(message.Value);
        });

        messenger.Register<TaskDeletedMessage>(this, (recipient, message) =>
        {
            OnTaskDeleted(message.Value);
        });
    }
    
    private static void OnTasksChanged(BindableObject bindable, object oldValue, object newValue)
    {
        Debug.WriteLine($"----[TaskListView] OnTasksChanged: bindable={bindable.GetType()}");
    }
    
    private async void OnTaskSelected(object sender, SelectionChangedEventArgs e)
    {
        if (e.CurrentSelection.Count == 0)
            return;

        var selectedTask = e.CurrentSelection[0] as TaskViewModel; 

        if (selectedTask != null)
        {
            Debug.WriteLine($"----[TaskListViewModel] Selected Task: {selectedTask.Text}");
            await Shell.Current.GoToAsync($"{nameof(Views.TaskPage)}?load={selectedTask.Id}");
        }

        // Deselect the item
        ((CollectionView)sender).SelectedItem = null;
    }

    void OnTaskDeleted(string taskId)
    {
        TaskViewModel? matchedTask = this.Tasks.FirstOrDefault(n => n.Id == taskId);

        if (matchedTask != null)
            Tasks.Remove(matchedTask);
    }

    void OnTaskSaved(string taskId)
    {
        TaskViewModel? matchedTask = Tasks.FirstOrDefault(n => n.Id == taskId);

        if (matchedTask != null)
        {
            matchedTask.Reload();
            Tasks.Move(Tasks.IndexOf(matchedTask), 0);
        }
            
        else 
            Tasks.Insert(0, TaskViewModelFactory.Create(TasksService.GetTask(taskId)));
    }
}