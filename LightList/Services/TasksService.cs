using System.Text.Json;
using CommunityToolkit.Mvvm.Messaging;
using LightList.Messages;
using LightList.Repositories;
using LightList.Utils;
using Task = System.Threading.Tasks.Task;

namespace LightList.Services;

public class TasksService : BaseService, ITasksService
{
    
    #region fields

    private readonly IMessenger _messenger;
    private readonly ILocalRepository _localRepository;
    private readonly ISyncService _syncService;

    #endregion
    
    #region ctor

    public TasksService(
        IMessenger messenger, 
        ILogger logger, 
        ILocalRepository localRepository,
        ISyncService syncService) : base(logger)
    {
        _messenger = messenger;
        _localRepository = localRepository;
        _syncService = syncService;
    }

    #endregion

    #region Public methods - Tasks

    public async Task<Models.Task> GetTask(string taskId)
    {
        _logger.Debug($"Getting task id={taskId}");
        return await ExecuteWithLogging(async () => await _localRepository.GetTask(taskId), "Error retrieving task");
    }

    public async Task<List<Models.Task>> GetTasks()
    {
        _logger.Debug("Getting all tasks");
        return await ExecuteWithLogging(async () => await _localRepository.GetAllTasks(), "Error retrieving all tasks");
    }

    public async Task SaveTask(Models.Task task)
    {
        _logger.Debug($"Saving task (id={task.Id})");
        await ExecuteWithLogging(async () =>
        {
            // save locally
            task.UpdatedAt = DateTime.Now;
            task.IsSynced = false;
            await _localRepository.SaveTask(task);

            // save remotely
            await _syncService.PushChangesAsync([task]);

            // update sync status
            task.IsSynced = true;
            await _localRepository.SaveTask(task);
        }, "Error saving task");
    }

    public async Task DeleteTask(Models.Task task)
    {
        _logger.Debug($"Marking task deleted (id={task.Id})");
        task.IsDeleted = true;
        await ExecuteWithLogging(async () => await SaveTask(task), "Error marking task deleted");
    }

    #endregion

    #region Public methods - Labels

    public async Task<List<Models.Label>> GetLabels()
    {
        _logger.Debug("Getting all labels");
        return await ExecuteWithLogging(async () => await _localRepository.GetAllLabels(), "Error getting all labels");
    }

    public async Task SaveLabel(Models.Label label)
    {
        _logger.Debug($"Saving label '{label.Name}'");
        
        await ExecuteWithLogging(async () =>
        {
            // save locally
            label.UpdatedAt = DateTime.Now;
            label.IsSynced = false;
            await _localRepository.SaveLabel(label);

            // save remotely TODO
            // await _syncService.PushChangesAsync([task]);

            // update sync status
            // label.IsSynced = true;
            // await _localRepository.SaveLabel(label);
        }, "Error saving label");
    }

    public async Task DeleteLabel(Models.Label label)
    {
        _logger.Debug($"Marking label '{label.Name}' deleted");
        label.IsDeleted = true;
        await ExecuteWithLogging(async () => await SaveLabel(label), "Error while deleting label");
    }

    #endregion
    
    #region Public methods - sync

    public async Task SyncNowAsync()
    {
        _logger.Debug("Syncing tasks");

        await ExecuteWithLogging(async () => await PushTaskChangesAsync(), "Error syncing tasks to remote");
        await ExecuteWithLogging(async () => await PullTaskChangesAsync(), "Error syncing tasks from remote");

        _logger.Debug("Finished syncing tasks");
    }

    #endregion

    #region utils
    
    private async Task PushTaskChangesAsync()
    {
        // Retrieve all tasks that have not been synced
        _logger.Debug("Pushing un-synced tasks");
        List<Models.Task> tasks = await _localRepository.GetAllTasks(true, false);

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
            await _localRepository.SaveTask(task);
        }
    }

    private async Task PullTaskChangesAsync()
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
                await _localRepository.SaveTask(task);
            }

        // Notify listeners that tasks have changed
        _messenger.Send(new TasksSyncedMessage(true));

        _logger.Debug("Tasks saved to local database");
    }

    #endregion
}