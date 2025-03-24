using System.Reflection;
using System.Text.Json;
using LightList.Models;
using LightList.Utils;
using Task = System.Threading.Tasks.Task;

namespace LightList.Repositories;

public class SecureStorageRepository: ISecureStorageRepository
{
    public SecureStorageRepository(){}

    public async Task SaveAuthTokensAsync(AuthTokens tokens)
    {
        Logger.Log($"Saving auth tokens");
        string tokensString = JsonSerializer.Serialize(tokens);
        await SecureStorage.SetAsync("AuthTokens", tokensString);
    }

    public async Task<AuthTokens?> GetAuthTokensAsync()
    {
        Logger.Log("Retrieving auth tokens");
        string? tokensString = await SecureStorage.GetAsync("AuthTokens");
        Logger.Log($"Auth tokens found: {string.IsNullOrEmpty(tokensString)}");
        return string.IsNullOrEmpty(tokensString) ? null : JsonSerializer.Deserialize<AuthTokens>(tokensString);
    }

    public void DeleteAuthTokens()
    {
        Logger.Log("Deleting auth tokens");
        SecureStorage.Remove("AuthTokens");
    }
}