using LightList.Models;
using LightList.Repositories;
using LightList.Utils;
using Task = System.Threading.Tasks.Task;

namespace LightList.Services;

public class SyncService : ISyncService
{
    private readonly IAuthService _authService;
    private readonly ILogger _logger;
    private readonly IRemoteRepository _remoteRepository;
    private readonly ISecureStorageRepository _secureStorage;

    public SyncService(
        ILogger logger,
        IAuthService authService,
        ISecureStorageRepository secureStorage,
        IRemoteRepository remoteRepo)
    {
        _logger = logger;
        _authService = authService;
        _secureStorage = secureStorage;
        _remoteRepository = remoteRepo;
    }

    #region Public Methods

    public async Task PushChangesAsync(List<Models.Task> tasks)
    {
        _logger.Debug("Pushing changes");
        await HandlePushQuery(tokens => PushUpdatedTasksAsync(tokens, tasks));
    }

    public async Task<List<Models.Task?>> PullChangesAsync()
    {
        _logger.Debug("Pulling changes");
        return await HandlePullQuery(GetUpdatedTasksAsync);
    }

    #endregion

    #region Utils

    private async Task HandlePushQuery(Func<AuthTokens, Task> action)
    {
        await HandleQuery(action);
    }

    private async Task<List<Models.Task?>> HandlePullQuery(Func<AuthTokens, Task<List<Models.Task?>>> action)
    {
        List<Models.Task?> result = [];
        await HandleQuery(async tokens => result = await action(tokens));
        return result;
    }

    private async Task HandleQuery(Func<AuthTokens, Task> action)
    {
        _logger.Debug($"Executing {action.Method.Name}");

        if (!await _authService.IsUserLoggedIn())
        {
            _logger.Debug("Sync skipped: User is not signed in.");
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
            _logger.Error($"Error synchronising: {ex.GetType().FullName}: {ex.Message}");
            throw;
        }

        _logger.Debug($"Finished executing {action.Method.Name}");
    }

    private async Task PushUpdatedTasksAsync(AuthTokens accessToken, List<Models.Task> tasks)
    {
        _logger.Debug("Pushing updated tasks");

        if (tasks.Count == 0)
        {
            _logger.Debug("No un-synced tasks found. Skipping push");
            return;
        }

        _logger.Debug($"Pushing {tasks.Count} tasks to remote");

        foreach (var task in tasks)
        {
            _logger.Debug($"Pushing task {task.Id}");

            // TODO: try-catch? If failed, continue with remaining items? 
            await _remoteRepository.PushUserTask(accessToken, task);
        }

        _logger.Debug($"Finished pushing {tasks.Count} tasks");
    }

    private async Task<List<Models.Task?>> GetUpdatedTasksAsync(AuthTokens accessToken)
    {
        _logger.Debug("Retrieving updated tasks");

        // Record current time to update secure storage once sync is finished
        var syncStartTime = DateTime.UtcNow;

        // Retrieve new tasks from remote db - new = Updated time >= secure storage sync time
        List<Models.Task?> tasks = await _remoteRepository.GetUserTasks(
            accessToken,
            await _secureStorage.GetLastSyncDateAsync());

        _logger.Debug($"Retrieved {tasks.Count} tasks");

        // Update sync time in secure storage
        await _secureStorage.SaveLastSyncDateAsync(syncStartTime);

        return tasks;
    }

    #endregion
}