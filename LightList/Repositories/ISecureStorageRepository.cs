using LightList.Models;
using Task = System.Threading.Tasks.Task;

namespace LightList.Repositories;

public interface ISecureStorageRepository
{ 
    Task SaveAuthTokensAsync(AuthTokens tokens);
    Task<AuthTokens?> GetAuthTokensAsync();
    void DeleteAuthTokens();
}