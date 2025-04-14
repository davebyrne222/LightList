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

    public async Task PushTasksAsync(List<Models.Task> tasks)
    {
        _logger.Debug("Pushing tasks");
        await HandlePushQuery(tokens => PushUpdatedItemsAsync(_remoteRepository.PushUserTask, tokens, tasks));
    }

    public async Task<List<Models.Task?>> PullTasksAsync()
    {
        _logger.Debug("Pulling tasks");
        return await HandlePullQuery<Models.Task>(tokens => GetUpdatedItemsAsync(_remoteRepository.GetUserTasks, tokens));
    }
    
    public async Task PushLabelsAsync(List<Models.Label> labels)
    {
        _logger.Debug("Pushing labels");
        await HandlePushQuery(tokens => PushUpdatedItemsAsync(_remoteRepository.PushUserLabel, tokens, labels));
    }

    public async Task<List<Models.Label?>> PullLabelsAsync()
    {
        _logger.Debug("Pulling labels");
        return await HandlePullQuery<Models.Label>(tokens => GetUpdatedItemsAsync(_remoteRepository.GetUserLabels, tokens));
    }

    #endregion

    #region Utils
    
    private async Task PushUpdatedItemsAsync<T>(Func<AuthTokens, T, Task> action, AuthTokens accessToken, List<T> items)
    {
        _logger.Debug("Pushing updated items");

        if (items.Count == 0)
        {
            _logger.Debug("No un-synced tasks found. Skipping push");
            return;
        }

        _logger.Debug($"Pushing {items.Count} items to remote");

        foreach (var task in items)
        {
            _logger.Debug($"Pushing item");

            // TODO: try-catch? If failed, continue with remaining items? 
            await action(accessToken, task);
        }

        _logger.Debug($"Finished pushing {items.Count} tasks");
    }

    private async Task<List<T?>> GetUpdatedItemsAsync<T>(Func<AuthTokens, DateTime?, Task<List<T?>>> action, AuthTokens accessToken)
    {
        _logger.Debug("Retrieving updated items");

        // Retrieve new tasks from remote db - new = Updated time >= secure storage sync time
        List<T?> items = await action(
            accessToken,
            await _secureStorage.GetLastSyncDateAsync());

        _logger.Debug($"Retrieved {items.Count} items");

        return items;
    }

    private async Task HandlePushQuery(Func<AuthTokens, Task> action)
    {
        await HandleQuery(action);
    }

    private async Task<List<T?>> HandlePullQuery<T>(Func<AuthTokens, Task<List<T?>>> action)
    {
        List<T?> result = [];
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
    #endregion
}