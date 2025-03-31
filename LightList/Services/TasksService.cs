using LightList.Models;
using LightList.Repositories;
using LightList.Utils;
using Task = System.Threading.Tasks.Task;

namespace LightList.Services;

public class TasksService: ITasksService
{
    private readonly ILocalRepository _localRepository;
    private readonly ISyncService _syncService;

    public TasksService(ILocalRepository localRepository, ISyncService syncService)
    {
        _localRepository = localRepository;
        _syncService = syncService;
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
        await _syncService.PushChangesAsync(); // TODO: change for method to push single item
        return await _localRepository.Save(task);
    }

    public void DeleteTask(Models.Task task)
    {
        Logger.Log($"Deleting task (id={task.Id})");
        _localRepository.Delete(task);
    }
    
    public async Task SyncNowAsync()
    {
        Logger.Log($"Syncing tasks");
        await _syncService.PushChangesAsync();
        await _syncService.PullChangesAsync();
        Logger.Log($"Finished syncing tasks");
    }
}