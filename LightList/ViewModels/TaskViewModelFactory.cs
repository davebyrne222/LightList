using LightList.Services;

namespace LightList.ViewModels;

public class TaskViewModelFactory: ITaskViewModelFactory
{
    private readonly ITasksService _tasksService;

    public TaskViewModelFactory(ITasksService tasksService)
    {
        _tasksService = tasksService;
    }

    public TaskViewModel Create(Models.Task task)
    {
        return new TaskViewModel(_tasksService, task);
    }
}