using System.Collections.ObjectModel;
using LightList.Services;
using LightList.Utils;

namespace LightList.ViewModels;

public class AllTasksViewModel: BaseTasksViewModel
{ 
    public AllTasksViewModel(ITaskViewModelFactory taskViewModelFactory, ITasksService tasksService) : base(taskViewModelFactory, tasksService)
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