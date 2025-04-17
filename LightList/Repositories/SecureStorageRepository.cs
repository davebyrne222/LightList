using System.Globalization;
using System.IdentityModel.Tokens.Jwt;
using System.Text.Json;
using LightList.Models;
using LightList.Utils;
using Task = System.Threading.Tasks.Task;

namespace LightList.Repositories;

public class SecureStorageRepository : ISecureStorageRepository
{
    private readonly ILogger _logger;

    public SecureStorageRepository(ILogger logger)
    {
        _logger = logger;
        DeleteAuthTokens();
    }

    #region Public methods - Auth

    public async Task<bool> SaveAuthTokensAsync(string tokensString)
    {
        _logger.Debug("Saving auth tokens");

        if (string.IsNullOrWhiteSpace(tokensString) || !IsValidTokenString(tokensString))
            throw new ArgumentException("Invalid token string format. Must be convertable to type AuthTokens",
                nameof(tokensString));

        try
        {
            await SecureStorage.SetAsync("AuthTokens", tokensString);
            _logger.Debug("Auth tokens saved");
            return true;
        }
        catch (Exception ex)
        {
            _logger.Error($"Failed to save auth tokens: {ex.GetType()} - {ex.Message}");
            throw;
        }
    }

    public async Task<AuthTokens?> GetAuthTokensAsync()
    {
        _logger.Debug("Retrieving auth tokens");

        var tokensString = await SecureStorage.GetAsync("AuthTokens");

        _logger.Debug($"Auth tokens found: {!string.IsNullOrEmpty(tokensString)}");

        if (string.IsNullOrEmpty(tokensString))
            return null;

        var tokens = JsonSerializer.Deserialize<AuthTokens>(tokensString);

        if (tokens == null)
            return null;

        _logger.Debug("Extracting user id");
        tokens.UserId = GetCognitoUserId(tokens.IdToken);

        return tokens;
    }

    public void DeleteAuthTokens()
    {
        _logger.Debug("Deleting auth tokens");
        SecureStorage.Remove("AuthTokens");
    }

    #endregion

    #region Public methods - Sync

    public async Task SaveLastSyncDateAsync(DateTime lastSyncDate)
    {
        _logger.Debug($"Saving last sync date: {lastSyncDate}");
        await SecureStorage.SetAsync("LastSyncDate", lastSyncDate.ToString(CultureInfo.CurrentCulture));
    }

    public async Task<DateTime?> GetLastSyncDateAsync()
    {
        _logger.Debug("Retrieving last sync date");
        var dateString = await SecureStorage.GetAsync("LastSyncDate");
        return DateTime.TryParse(dateString, out var result) ? result : null;
    }

    public void DeleteLastSyncDate()
    {
        _logger.Debug("Deleting last sync date");
        SecureStorage.Remove("LastSyncDate");
    }

    #endregion

    #region Utils

    private bool IsValidTokenString(string json)
    {
        try
        {
            var tokens = JsonSerializer.Deserialize<AuthTokens>(json);
            return tokens != null;
        }
        catch
        {
            _logger.Error($"Token is invalid format: {json}");
            return false;
        }
    }

    private string GetCognitoUserId(string jwtToken)
    {
        _logger.Debug("Extracting cognito user id");
        var handler = new JwtSecurityTokenHandler();
        var token = handler.ReadJwtToken(jwtToken);
        return token.Claims.First(claim => claim.Type == "sub").Value;
    }

    #endregion
}