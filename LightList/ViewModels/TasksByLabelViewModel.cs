using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.Messaging;
using LightList.Services;
using LightList.Utils;

namespace LightList.ViewModels;

public class TasksByLabelViewModel: BaseTasksViewModel
{ 
    public TasksByLabelViewModel(ITaskViewModelFactory taskViewModelFactory, ITasksService tasksService, IMessenger messenger) : base(taskViewModelFactory, tasksService, messenger)
    {
        _ = InitializeTasks();
    }

    private async Task InitializeTasks()
    {
        var tasks = await TasksService.GetTasks();
        AllTasks = new ObservableCollection<TaskViewModel>(tasks.Select(n => TaskViewModelFactory.Create(n)));
        Logger.Log($"Retrieved {AllTasks.Count} tasks");

    }
}