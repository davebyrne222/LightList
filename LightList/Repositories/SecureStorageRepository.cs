using System.Text.Json;
using LightList.Models;
using LightList.Utils;
using Task = System.Threading.Tasks.Task;

namespace LightList.Repositories;

public class SecureStorageRepository: ISecureStorageRepository
{
    
    #region Public methods
    public async Task SaveAuthTokensAsync(string tokensString)
    {
        Logger.Log("Saving auth tokens");

        if (string.IsNullOrWhiteSpace(tokensString) || !IsValidTokenString(tokensString))
            throw new ArgumentException("Invalid token string format. Must be convertable to type AuthTokens", nameof(tokensString));
        
        await SecureStorage.SetAsync("AuthTokens", tokensString);
    }

    public async Task<AuthTokens?> GetAuthTokensAsync()
    {
        Logger.Log("Retrieving auth tokens");
        
        string? tokensString = await SecureStorage.GetAsync("AuthTokens");
        
        Logger.Log($"Auth tokens found: {!string.IsNullOrEmpty(tokensString)}");
        
        return string.IsNullOrEmpty(tokensString) ? null : JsonSerializer.Deserialize<AuthTokens>(tokensString);
    }
    
    public void DeleteAuthTokens()
    {
        Logger.Log("Deleting auth tokens");
        SecureStorage.Remove("AuthTokens");
    }
    
    #endregion
    
    #region Utils
    private bool IsValidTokenString(string json)
    {
        try
        {
            AuthTokens? tokens = JsonSerializer.Deserialize<AuthTokens>(json);
            return tokens != null;
        }
        catch
        {
            return false;
        }
    }
    #endregion
}