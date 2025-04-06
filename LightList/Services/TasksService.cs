using CommunityToolkit.Mvvm.Messaging;
using LightList.Messages;
using LightList.Repositories;
using LightList.Utils;
using Task = System.Threading.Tasks.Task;

namespace LightList.Services;

public class TasksService : ITasksService
{
    #region ctor

    public TasksService(IMessenger messenger, ILogger logger, ILocalRepository localRepository,
        ISyncService syncService)
    {
        _messenger = messenger;
        _logger = logger;
        _localRepository = localRepository;
        _syncService = syncService;
    }

    #endregion

    #region fields

    private readonly IMessenger _messenger;
    private readonly ILocalRepository _localRepository;
    private readonly ILogger _logger;
    private readonly ISyncService _syncService;

    #endregion

    #region Publilc methods

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
        _logger.Debug($"Saving task (id={task.Id})");

        // update metadata
        task.UpdatedAt = DateTime.Now;
        task.IsSynced = false;

        try
        {
            // save remotely
            await SaveRemote(task);
            task.IsSynced = true;
        }
        catch (Exception ex)
        {
            _logger.Error($"Error saving task to remote: {ex.GetType()} - {ex.Message}");
        }
        finally
        {
            // save locally
            await SaveLocally(task);
        }

        return task.Id;
    }

    public async Task DeleteTask(Models.Task task)
    {
        _logger.Debug($"Marking task deleted (id={task.Id})");

        // update meta data
        task.IsDeleted = true;

        // save
        await SaveTask(task);
    }

    public async Task SyncNowAsync()
    {
        _logger.Debug("Syncing tasks");

        try
        {
            await PushChangesAsync();
        }
        catch (Exception ex)
        {
            _logger.Error($"Error syncing tasks to remote: {ex.GetType()} - {ex.Message}");
            throw;
        }

        try
        {
            await PullChangesAsync();
        }
        catch (Exception ex)
        {
            _logger.Error($"Error synching tasks from remote: {ex.GetType()} - {ex.Message}");
            throw;
        }

        _logger.Debug("Finished syncing tasks");
    }

    #endregion

    #region utils

    private async Task SaveLocally(Models.Task task)
    {
        _logger.Debug($"Saving task (id={task.Id})");

        try
        {
            await _localRepository.Save(task);
        }
        catch (Exception ex)
        {
            _logger.Error($"Unexpected error while saving task: {ex.GetType()} - {ex.Message}");
            throw;
        }
    }

    private async Task SaveRemote(Models.Task task)
    {
        _logger.Debug($"Pushing task to remote (id={task.Id})");
        try
        {
            await _syncService.PushChangesAsync([task]);
        }
        catch (Exception ex)
        {
            _logger.Error($"Error pushing task to remote: {ex.GetType()} - {ex.Message}");
            throw;
        }
    }

    private async Task PushChangesAsync()
    {
        // Retrieve all tasks that have not been synced
        _logger.Debug("Pushing un-synced tasks");
        List<Models.Task> tasks = await _localRepository.GetAll(true, false);

        if (tasks.Count == 0)
        {
            _logger.Debug("No tasks found");
            return;
        }

        _logger.Debug($"Found {tasks.Count} un-synced tasks. Pushing");

        // Push to remote
        await _syncService.PushChangesAsync(tasks);

        // Update sync status in local database
        foreach (var task in tasks)
        {
            task.IsSynced = true;
            await SaveLocally(task);
        }
    }

    private async Task PullChangesAsync()
    {
        // Pull all new/updated tasks from remote
        _logger.Debug("Pulling new/updated tasks");
        var tasks = await _syncService.PullChangesAsync();

        if (tasks.Count == 0)
        {
            _logger.Debug("No tasks found");
            return;
        }

        _logger.Debug($"Retrieved {tasks.Count} tasks. Saving locally");

        // Store to local db
        foreach (var task in tasks)
            if (task != null)
            {
                task.IsSynced = true;
                await SaveLocally(task);
            }

        // Notify listeners that tasks have changed
        _messenger.Send(new TasksSyncedMessage(true));

        _logger.Debug("Tasks saved to local database");
    }

    #endregion
}