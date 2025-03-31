using System.Text.Json;
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
    
    public SyncService(IAuthService authService, ISecureStorageRepository secureStorage, ILocalRepository localRepo, IRemoteRepository remoteRepo)
    {
        _authService = authService;
        _secureStorage = secureStorage;
        _localRepository = localRepo;
        _remoteRepository = remoteRepo;
    }
    
    #region Public Methods

    public async Task PushChangesAsync()
    {
        if (!await _authService.IsUserLoggedIn()) 
        {
            Console.WriteLine("Sync skipped: User is not signed in.");
            return;
        }
        
        Logger.Log("Pushing Data");
        
        try
        {
            AuthTokens? accessToken = await _secureStorage.GetAuthTokensAsync();
            
            if (accessToken == null)
                throw new UnauthorizedAccessException("Failed to get access token");

            // TODO: get only non-synched tasks
            List<Models.Task> tasks = await _localRepository.GetAll();
            
            await _remoteRepository.PushUserTasks(accessToken, tasks);
            
            Logger.Log($"Pushed {tasks.Count} tasks");
        }
        catch (Exception ex)
        {
            Logger.Log($"Error synchronising: {ex.GetType().FullName}: {ex.Message}");
            throw;
        }
        
        Logger.Log("Remote database synchronized");
    }

    public async Task PullChangesAsync()
    {
        Logger.Log("Pulling Remote Data");

        if (!await _authService.IsUserLoggedIn()) 
        {
            Console.WriteLine("Sync skipped: User is not signed in.");
            return;
        }
        
        try
        {
            AuthTokens? accessToken = await _secureStorage.GetAuthTokensAsync();
            
            if (accessToken == null)
                throw new UnauthorizedAccessException("Failed to get access token");
            
            // TODO: get only non-synched tasks
            List<Models.Task?> tasks = await _remoteRepository.GetUserTasks(accessToken);
            
            Logger.Log($"Retrieved: {tasks.Count} tasks");

            foreach (var task in tasks)
            {
                if (task != null) await _localRepository.Save(task);
            }
            
        }
        catch (Exception ex)
        {
            Logger.Log($"Error synchronising: {ex.GetType().FullName}: {ex.Message}");
            throw;
        }
        
        Logger.Log("Database synchronized");
    }
    
    #endregion
}