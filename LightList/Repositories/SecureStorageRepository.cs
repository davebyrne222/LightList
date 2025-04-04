using System.Globalization;
using System.IdentityModel.Tokens.Jwt;
using System.Text.Json;
using LightList.Models;
using LightList.Utils;
using Task = System.Threading.Tasks.Task;

namespace LightList.Repositories;

public class SecureStorageRepository: ISecureStorageRepository
{
    
    #region Public methods - Auth
    public async Task<bool> SaveAuthTokensAsync(string tokensString)
    {
        Logger.Log("Saving auth tokens");

        if (string.IsNullOrWhiteSpace(tokensString) || !IsValidTokenString(tokensString))
            throw new ArgumentException("Invalid token string format. Must be convertable to type AuthTokens", nameof(tokensString));
        
        try
        {
            await SecureStorage.SetAsync("AuthTokens", tokensString);
            Logger.Log("Auth tokens saved");
            return true;
        }
        catch (Exception ex)
        {
            Logger.Log($"Failed to save auth tokens: {ex.GetType()} - {ex.Message}");
            throw;
        }
    }

    public async Task<AuthTokens?> GetAuthTokensAsync()
    {
        Logger.Log("Retrieving auth tokens");
        
        string? tokensString = await SecureStorage.GetAsync("AuthTokens");
        
        Logger.Log($"Auth tokens found: {!string.IsNullOrEmpty(tokensString)}");
        
        if (string.IsNullOrEmpty(tokensString))
            return null;
        
        AuthTokens? tokens = JsonSerializer.Deserialize<AuthTokens>(tokensString);
        
        if (tokens == null)
            return null;
        
        Logger.Log("Extracting user id");
        tokens.UserId = GetCognitoUserId(tokens.IdToken);
        
        return tokens;
    }
    
    public void DeleteAuthTokens()
    {
        Logger.Log("Deleting auth tokens");
        SecureStorage.Remove("AuthTokens");
    }
    
    #endregion
    
    #region Public methods - Sync
    public async Task SaveLastSyncDateAsync(DateTime lastSyncDate)
    {
        Logger.Log($"Saving last sync date: {lastSyncDate}");
        await SecureStorage.SetAsync("LastSyncDate", lastSyncDate.ToString(CultureInfo.CurrentCulture));
    }

    public async Task<DateTime?> GetLastSyncDateAsync()
    {
        Logger.Log("Retrieving last sync date");
        string? dateString = await SecureStorage.GetAsync("LastSyncDate");
        return DateTime.TryParse(dateString, out DateTime result) ? result : (DateTime?)null;
    }

    public void DeleteLastSyncDate()
    {
        Logger.Log("Deleting last sync date");
        SecureStorage.Remove("LastSyncDate");
    }
    #endregion
    
    #region Utils
    private static bool IsValidTokenString(string json)
    {
        try
        {
            AuthTokens? tokens = JsonSerializer.Deserialize<AuthTokens>(json);
            return tokens != null;
        }
        catch
        {
            Logger.Log($"Token is invalid format: {json}");
            return false;
        }
    }
    
    private static string GetCognitoUserId(string jwtToken)
    {
        Logger.Log("Extracting cognito user id");
        var handler = new JwtSecurityTokenHandler();
        var token = handler.ReadJwtToken(jwtToken);
        return token.Claims.First(claim => claim.Type == "sub").Value;
    }
    #endregion
}