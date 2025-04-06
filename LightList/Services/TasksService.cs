using CommunityToolkit.Mvvm.Messaging;
using LightList.Messages;
using LightList.Repositories;
using LightList.Utils;
using Task = System.Threading.Tasks.Task;

namespace LightList.Services;

public class TasksService : ITasksService
{
    #region fields
    private readonly IMessenger _messenger;
    private readonly ILocalRepository _localRepository;
    private readonly ILogger _logger;
    private readonly ISyncService _syncService;
    #endregion
    
    #region ctor
    public TasksService(IMessenger messenger, ILogger logger, ILocalRepository localRepository, ISyncService syncService)
    {
        _messenger = messenger;
        _logger = logger;
        _localRepository = localRepository;
        _syncService = syncService;
    }
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
        task.IsSynced = false;
        
        // save
        return await _SaveTask(task);
    }
    
    public async Task DeleteTask(Models.Task task)
    {
        _logger.Debug($"Marking task deleted (id={task.Id})");
        
        // update meta data
        task.IsDeleted = true;
        task.IsSynced = false;
        
        // save
        await _SaveTask(task);
    }

    public async Task SyncNowAsync()
    {
        _logger.Debug("Syncing tasks");
        
        await _syncService.PushChangesAsync();
        await _syncService.PullChangesAsync();
        
        _messenger.Send(new TasksSyncedMessage(true));

        _logger.Debug("Finished syncing tasks");
    }
    #endregion
    
    #region utils
    
    private async Task<string> _SaveTask(Models.Task task)
    {
        _logger.Debug($"Saving task (id={task.Id})");
        
        // update metadata
        task.UpdatedAt = DateTime.Now;
        
        try
        {
            // save remotely
            _logger.Debug($"Pushing task to remote (id={task.Id})");
            await _syncService.PushChangesAsync();
            task.IsSynced = true;
        }
        catch (Exception ex)
        {
            _logger.Error($"Error pushing task to remote: {ex.GetType()} - {ex.Message}");
            throw;
        }
        finally
        {
            // save locally
            await _localRepository.Save(task);   
        }
        
        return task.Id;
    }
    
    #endregion
}