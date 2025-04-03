using System;
using System.Text.Json;
using Amazon.AppSync.Model;
using LightList.Models;
using LightList.Repositories;
using LightList.Utils;
using Task = System.Threading.Tasks.Task;

namespace LightList.Services;

public class SyncService : ISyncService
{
    private readonly IAuthService _authService;
    private readonly ISecureStorageRepository _secureStorage;
    private readonly ILocalRepository _localRepository;
    private readonly IRemoteRepository _remoteRepository;
    
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
        Logger.Log("Changes pushed");
    }

    public async Task PullChangesAsync()
    {
        Logger.Log("Pulling changes");
        await HandleQuery(GetUpdatedTasksAsync);
        Logger.Log("Changes pulled");
    }
    
    #endregion

    #region Utils

    private async Task HandleQuery(Func<AuthTokens, Task> action)
    {
        Logger.Log($"Executing {action}");

        if (!await _authService.IsUserLoggedIn()) 
        {
            Logger.Log("Sync skipped: User is not signed in.");
            return;
        }
        
        try
        {
            AuthTokens? accessTokens = await _secureStorage.GetAuthTokensAsync();
            
            if (accessTokens == null)
                throw new UnauthorizedAccessException("Failed to get access token");
            
            await action(accessTokens);
            
        }
        catch (Exception ex)
        {
            Logger.Log($"Error synchronising: {ex.GetType().FullName}: {ex.Message}");
            throw;
        }
    }

    private async Task GetUpdatedTasksAsync(AuthTokens accessToken)
    {
        Logger.Log("Retrieving updated tasks");

        List<Models.Task?> tasks = await _remoteRepository.GetUserTasks(
            accessToken,
            await _secureStorage.GetLastSyncDateAsync());
        
        // Update last sync time in secure storage
        await _secureStorage.SaveLastSyncDateAsync(DateTime.UtcNow);
        
        Logger.Log($"Retrieved {tasks.Count} tasks");

        foreach (var task in tasks)
        {
            if (task != null) await _localRepository.Save(task);
        }
        
        Logger.Log($"Tasks saved to local database");
    }

    private async Task PushUpdatedTasksAsync(AuthTokens accessToken)
    {
        Logger.Log("Pushing updated tasks");

        // Retrieve all tasks that have not been synced
        List<Models.Task> tasks = await _localRepository.GetAll(true);
        
        Logger.Log($"Retrieved {tasks.Count} un-synced tasks. Pushing to remote");
        
        // Sync each task
        foreach (var task in tasks)
        {
            await _remoteRepository.PushUserTask(accessToken, task);
            
            task.IsPushedToRemote = true;
            
            await _localRepository.Save(task);
        }

        Logger.Log($"Pushed {tasks.Count} tasks");
    }
    
    #endregion
}