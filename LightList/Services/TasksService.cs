using LightList.Models;
using LightList.Repositories;
using LightList.Utils;

namespace LightList.Services;

public class TasksService: ITasksService
{
    private readonly ILocalRepository _localRepository;

    public TasksService(ILocalRepository localRepository)
    {
        _localRepository = localRepository;
    }

    public async Task<Models.Task> GetTask(int taskId)
    {
        Logger.Log($"Getting task (id={taskId})");
        return await _localRepository.Get(taskId);
    }

    public async Task<List<Models.Task>> GetTasks()
    { 
        Logger.Log($"Getting all tasks");
        return await _localRepository.GetAll();
    }

    public async Task<int> SaveTask(Models.Task task)
    {
        Logger.Log($"Saving task (id={task.Id}, default id: {task.Id == default})");
        return await _localRepository.Save(task);
    }

    public void DeleteTask(Models.Task task)
    {
        Logger.Log($"Deleting task (id={task.Id})");
        _localRepository.Delete(task);
    }
}