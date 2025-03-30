using System.Text.Json;
using LightList.Models;
using LightList.Repositories;
using LightList.Utils;
using Task = System.Threading.Tasks.Task;

namespace LightList.Services;

public class SyncService : ISyncService
{
    private readonly ISecureStorageRepository _secureStorage;
    private readonly IRemoteRepository _remoteRepository;
    
    public SyncService(ISecureStorageRepository secureStorage, IRemoteRepository repository)
    {
        _secureStorage = secureStorage;
        _remoteRepository = repository;
    }
    
    #region Public Methods

    public async Task SyncRemoteDataAsync()
    {
        Logger.Log("Syncing Remote Data");
        
        try
        {
            string query =
                @"query { getUserTasks(UserId: ""a215d474-2001-7094-3233-f4c7dc35771f"") { ItemId, Data, UpdatedAt } }";
            
            Logger.Log($"Remote query: {query}");

            string accessToken = await GetUserAccessToken();
            List<Models.Task?> tasks = await _remoteRepository.ExecuteQuery(accessToken, query);
            
            Logger.Log($"Retrieved: {tasks.Count} tasks");
            //TODO: update local database
        }
        catch (Exception ex)
        {
            Logger.Log($"Error synchronising: {ex.GetType().FullName} - {ex.Message}");
            throw;
        }
        
        Logger.Log("Database synchronized");
    }
    
    #endregion
    
    #region Utils

    private async Task<string> GetUserAccessToken()
    {
        var result = await _secureStorage.GetAuthTokensAsync();
        return result.IdToken;
    }

    #endregion
}