using LightList.Repositories;
using LightList.Utils;
using Task = System.Threading.Tasks.Task;

namespace LightList.Services;

public class TasksService : ITasksService
{
    private readonly ILocalRepository _localRepository;
    private readonly ILogger _logger;
    private readonly ISyncService _syncService;

    public TasksService(ILogger logger, ILocalRepository localRepository, ISyncService syncService)
    {
        _logger = logger;
        _localRepository = localRepository;
        _syncService = syncService;
    }

    public async Task<Models.Task> GetTask(string taskId)
    {
        _logger.Debug($"Getting task id={taskId}");
        return await _localRepository.Get(taskId);
    }

    public async Task<List<Models.Task>> GetTasks()
    {
        _logger.Debug("Getting all tasks");
        return await _localRepository.GetAll();
    }

    public async Task<string> SaveTask(Models.Task task)
    {
        _logger.Debug($"Saving task id={task.Id}");
        var taskId = await _localRepository.Save(task);
        await _syncService.PushChangesAsync();
        return taskId;
    }

    public void DeleteTask(Models.Task task)
    {
        _logger.Debug($"Deleting task (id={task.Id})");
        _localRepository.Delete(task);
    }

    public async Task SyncNowAsync()
    {
        _logger.Debug("Syncing tasks");
        await _syncService.PushChangesAsync();
        await _syncService.PullChangesAsync();
        _logger.Debug("Finished syncing tasks");
    }
}