using LightList.Models;
using LightList.Repositories;
using LightList.Utils;
using Task = System.Threading.Tasks.Task;

namespace LightList.Services;

public class SyncService : ISyncService
{
    private readonly IAuthService _authService;
    private readonly ILocalRepository _localRepository;
    private readonly IRemoteRepository _remoteRepository;
    private readonly ISecureStorageRepository _secureStorage;

    public SyncService(
        IAuthService authService,
        ISecureStorageRepository secureStorage,
        ILocalRepository localRepo,
        IRemoteRepository remoteRepo)
    {
        _authService = authService;
        _secureStorage = secureStorage;
        _localRepository = localRepo;
        _remoteRepository = remoteRepo;
    }

    #region Public Methods

    public async Task PushChangesAsync()
    {
        Logger.Log("Pushing changes");
        await HandleQuery(PushUpdatedTasksAsync);
    }

    public async Task PullChangesAsync()
    {
        Logger.Log("Pulling changes");
        await HandleQuery(GetUpdatedTasksAsync);
    }

    #endregion

    #region Utils

    private async Task HandleQuery(Func<AuthTokens, Task> action)
    {
        Logger.Log($"Executing {action.Method.Name}");

        if (!await _authService.IsUserLoggedIn())
        {
            Logger.Log("Sync skipped: User is not signed in.");
            return;
        }

        try
        {
            var accessTokens = await _secureStorage.GetAuthTokensAsync();

            if (accessTokens == null)
                throw new UnauthorizedAccessException("Failed to get access token");

            await action(accessTokens);
        }
        catch (Exception ex)
        {
            Logger.Log($"Error synchronising: {ex.GetType().FullName}: {ex.Message}");
            throw;
        }
        
        Logger.Log($"Finished executing {action.Method.Name}");
    }

    private async Task GetUpdatedTasksAsync(AuthTokens accessToken)
    {
        Logger.Log("Retrieving updated tasks");

        // Record current time to update secure storage once sync is finished
        var syncStartTime = DateTime.UtcNow;

        // Retrieve new tasks from remote db - new = Updated time >= secure storage sync time
        List<Models.Task?> tasks = await _remoteRepository.GetUserTasks(
            accessToken,
            await _secureStorage.GetLastSyncDateAsync());

        Logger.Log($"Retrieved {tasks.Count} tasks");
        
        if (tasks.Count == 0)
            return;

        // Store to local db
        foreach (var task in tasks)
            if (task != null)
            {
                task.IsSynced = true;
                await _localRepository.Save(task);
            }

        // Update sync time in secure storage
        await _secureStorage.SaveLastSyncDateAsync(syncStartTime);

        Logger.Log("Tasks saved to local database");
    }

    private async Task PushUpdatedTasksAsync(AuthTokens accessToken)
    {
        Logger.Log("Pushing updated tasks");

        // Retrieve all tasks that have not been synced
        List<Models.Task> tasks = await _localRepository.GetAll(true);

        Logger.Log($"Retrieved {tasks.Count} un-synced tasks. Pushing to remote");

        // Sync each task: send to remote and set synced indicator locally
        foreach (var task in tasks)
        {
            await _remoteRepository.PushUserTask(accessToken, task);
            task.IsSynced = true;
            await _localRepository.Save(task);
        }

        Logger.Log($"Finished pushing {tasks.Count} tasks");
    }

    #endregion
}