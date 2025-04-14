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
    private readonly ISecureStorageRepository _secureStorage;
    private readonly ILocalRepository _localRepository;
    private readonly ISyncService _syncService;

    #endregion
    
    #region ctor

    public TasksService(
        IMessenger messenger, 
        ILogger logger,
        ISecureStorageRepository secureStorage,
        ILocalRepository localRepository,
        ISyncService syncService) : base(logger)
    {
        _messenger = messenger;
        _secureStorage = secureStorage;
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
            await _syncService.PushTasksAsync([task]);

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

            // save remotely
            await _syncService.PushLabelsAsync([label]);

            // update sync status
            label.IsSynced = true;
            await _localRepository.SaveLabel(label);
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
        _logger.Debug("Syncing with remote");
        
        // Record current time to update secure storage once sync is finished
        var syncStartTime = DateTime.UtcNow;
        
        await ExecuteWithLogging(async () => await PushLabelChangesAsync(), "Error syncing labels to remote");
        await ExecuteWithLogging(async () => await PullLabelChangesAsync(), "Error syncing labels from remote");
        await ExecuteWithLogging(async () => await PushTaskChangesAsync(), "Error syncing tasks to remote");
        await ExecuteWithLogging(async () => await PullTaskChangesAsync(), "Error syncing tasks from remote");
        
        // Update sync time in secure storage
        await _secureStorage.SaveLastSyncDateAsync(syncStartTime);

        _logger.Debug("Finished syncing");
    }

    #endregion

    #region utils
    
    private async Task PushLabelChangesAsync()
    {
        // Retrieve all tasks that have not been synced
        _logger.Debug("Pushing un-synced labels");
        List<Models.Label> labels = await _localRepository.GetAllLabels(true, false);

        if (labels.Count == 0)
        {
            _logger.Debug("No labels found");
            return;
        }

        _logger.Debug($"Found {labels.Count} un-synced labels. Pushing");

        // Push to remote
        await _syncService.PushLabelsAsync(labels);

        // Update sync status in local database
        foreach (var label in labels)
        {
            label.IsSynced = true;
            await _localRepository.SaveLabel(label);
        }
    }

    private async Task PullLabelChangesAsync()
    {
        // Pull all new/updated tasks from remote
        _logger.Debug("Pulling new/updated labels");
        var labels = await _syncService.PullLabelsAsync();
        
        if (labels.Count == 0)
        {
            _logger.Debug("No labels found");
            return;
        }

        _logger.Debug($"Retrieved {labels.Count} labels. Saving locally");

        // Store to local db
        foreach (var label in labels)
            if (label != null)
            {
                label.IsSynced = true;
                await _localRepository.SaveLabel(label);
            }

        // Notify listeners that tasks have changed
        _messenger.Send(new LabelsSyncedMessage(true));

        _logger.Debug("Labels saved to local database");
    }
    
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
        await _syncService.PushTasksAsync(tasks);

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
        var tasks = await _syncService.PullTasksAsync();

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