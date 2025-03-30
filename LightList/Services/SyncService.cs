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
    private readonly IRemoteRepository _remoteRepository;
    
    public SyncService(IAuthService authService, ISecureStorageRepository secureStorage, IRemoteRepository repository)
    {
        _authService = authService;
        _secureStorage = secureStorage;
        _remoteRepository = repository;
    }
    
    #region Public Methods

    public async Task PullChangesAsync()
    {
        if (!await _authService.IsUserLoggedIn()) 
        {
            Console.WriteLine("Sync skipped: User is not signed in.");
            return;
        }
        
        Logger.Log("Syncing Remote Data");
        
        try
        {
            AuthTokens? accessToken = await _secureStorage.GetAuthTokensAsync();
            
            if (accessToken == null)
                throw new UnauthorizedAccessException("Failed to get access token");
            
            List<Models.Task?> tasks = await _remoteRepository.GetUserTasks(accessToken);
            
            Logger.Log($"Retrieved: {tasks.Count} tasks");
            //TODO: update local database
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