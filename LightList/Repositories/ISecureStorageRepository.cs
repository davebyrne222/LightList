using Amazon.CognitoIdentityProvider.Model;
using LightList.Models;
using Task = System.Threading.Tasks.Task;

namespace LightList.Repositories;

public interface ISecureStorageRepository
{
    Task SaveAuthTokensAsync(string tokensString);
    Task<AuthTokens?> GetAuthTokensAsync();
    void DeleteAuthTokens();
    Task SaveLastSyncDateAsync(DateTime lastSyncDate);
    Task<DateTime?> GetLastSyncDateAsync();
    public void DeleteLastSyncDate();
}