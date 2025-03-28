using System.Net;
using System.Security.Authentication;
using System.IdentityModel.Tokens.Jwt;
using System.Text.Json;
using Amazon.CognitoIdentityProvider;
using Amazon.CognitoIdentityProvider.Model;
using Amazon.Runtime;
using CommunityToolkit.Maui.Core.Views;
using LightList.Models;
using LightList.Repositories;
using LightList.Utils;
using Task = System.Threading.Tasks.Task;

namespace LightList.Services;

public class AuthService: IAuthService
{
    private readonly ISecureStorageRepository _secureStorage;
    private AmazonCognitoIdentityProviderClient? _provider;
    private AmazonCognitoIdentityProviderClient Provider => _provider ??=
        new AmazonCognitoIdentityProviderClient(new AnonymousAWSCredentials(), Constants.AwsRegion);
    
    public AuthService(ISecureStorageRepository repository)
    {
        Logger.Log("Initializing");
        _secureStorage = repository;
        Logger.Log("Initialized");
    }

    #region Public Methods
    public async Task<bool> SignInAsync()
    {
        Logger.Log($"Redirecting to Cognito Auth: {new Uri(Constants.CognitoAuthUrl)}");

        try
        {
            var authResult = await WebAuthenticator.Default.AuthenticateAsync(
                new Uri(Constants.CognitoAuthUrl),
                new Uri(Constants.AuthRedirectUrl)
            );

            Logger.Log($"Authenticated: {authResult.Properties["code"] != null}");

            if (authResult.Properties["code"] != null)
                return await ExchangeCodeForTokensAsync(authResult.Properties["code"]);
        }
        catch (TaskCanceledException)
        {
            // User has cancelled login process; do nothing
        }
        catch (Exception ex)
        {
            Logger.Log($"Exception while signing in: {ex.GetType().FullName} - {ex.Message}");
        }
        
        return false;
    }

    public async Task<bool> SignOutAsync()
    {

        // Sign out from cognito
        Logger.Log($"Signing out: requesting cognito sign out");

        AuthTokens? tokens = await _secureStorage.GetAuthTokensAsync();
        
        if (tokens == null)
            return true;

        GlobalSignOutRequest request = new GlobalSignOutRequest
        {
            AccessToken = tokens?.AccessToken
        };

        try
        {
            await Provider.GlobalSignOutAsync(request);
            //TODO: redirect to /logout? https://docs.aws.amazon.com/cognito/latest/developerguide/amazon-cognito-user-pools-using-the-refresh-token.html
        }
        catch (Exception ex)
        {
            Logger.Log($"Error signing out from cognito: {ex.GetType().FullName} - {ex.Message}");
            return false;   
        }
        
        // Delete stored tokens
        _secureStorage.DeleteAuthTokens();

        return true;
    }

    #endregion

    public async Task<bool> IsUserLoggedIn()
    {
        Logger.Log("Checking if user is logged in");
        
        AuthTokens? authTokens = await _secureStorage.GetAuthTokensAsync();

        if (authTokens == null)
        {
            Logger.Log("No auth tokens found");
            return false;
        }
        
        // Check if access token is valid - if it is, user is logged in
        Logger.Log("Tokens found. Checking if expired");
        bool tokenExpired = IsTokenExpired(authTokens.AccessToken);

        if (!tokenExpired)
        {
            Logger.Log("Token valid. User is logged in");
            return true;
        }
        
        // If access token expired, refresh
        Logger.Log("Token expired. Requesting refresh");
        return await RefreshAccessTokenAsync(authTokens.RefreshToken);
    }

    #region Utils

    private async Task<bool> ExchangeCodeForTokensAsync(string authCode)
    {
        Logger.Log("Exchanging auth code for JWTs");
        
        // Exchange code
        var client = new HttpClient();
        var requestBody = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            { "grant_type", "authorization_code" },
            { "client_id", Constants.CognitoAppClientId },
            { "code", authCode },
            { "redirect_uri", Constants.AuthRedirectUrl }
        });

        HttpResponseMessage response = await client.PostAsync(Constants.CognitoTokenExchangeUrl, requestBody);

        if (!response.IsSuccessStatusCode)
        {
            Logger.Log($"Exchange failed: {response.StatusCode} - {response.ReasonPhrase}");
            return false;
        }
        
        Logger.Log($"Exchange successful. Saving tokens");
        
        // save tokens
        try
        {
            string authString = await response.Content.ReadAsStringAsync();
            return _secureStorage.SaveAuthTokensAsync(authString).IsCompletedSuccessfully;
        }
        catch (Exception ex)
        {
            Logger.Log($"Error storing access tokens: {ex.Message}");
            throw;
        }
    }

    private async Task<bool> RefreshAccessTokenAsync(string refreshToken)
    {
        Logger.Log("Refreshing access token");
        
        // Request new token
        var request = new InitiateAuthRequest
        {
            AuthFlow = AuthFlowType.REFRESH_TOKEN_AUTH,
            ClientId = Constants.CognitoAppClientId,
            AuthParameters = new Dictionary<string, string> { { "REFRESH_TOKEN", refreshToken } }
        };

        InitiateAuthResponse response;
        
        try
        {
            response = await Provider.InitiateAuthAsync(request);
        }
        catch (Exception ex)
        {
            Logger.Log($"Error refreshing auth tokens: {ex.GetType().FullName} - {ex.Message}");
            throw;
        }
        
        if (response.HttpStatusCode != HttpStatusCode.OK)
            throw new AuthenticationException($"Failed to refresh access token. HTTP Status: {response.HttpStatusCode} (Challenge: {response.ChallengeName})");
            
        if (response.AuthenticationResult == null)
            throw new AuthenticationException($"Failed to refresh access token. AuthenticationResult == null");
            
        // Save new token
        Logger.Log("Token refreshed. Saving");
        string tokenString = JsonSerializer.Serialize(response.AuthenticationResult);
        return _secureStorage.SaveAuthTokensAsync(tokenString).IsCompletedSuccessfully;

    }
    
    private static bool IsTokenExpired(string token)
    {
        Logger.Log("Checking if access token is expired");
        
        // Get 'exp' claim
        JwtSecurityTokenHandler handler = new();
        var jwtToken = handler.ReadJwtToken(token);
        var expiry = jwtToken.Claims.FirstOrDefault(c => c.Type == "exp")?.Value;

        // If no 'exp' claim, token is _likely_ valid (may not be on auth server)
        if (string.IsNullOrEmpty(expiry))
        {
            Logger.Log("No expiry claim found");
            return false;
        }

        // Check if exp has passed
        DateTime expDateTime = DateTimeOffset.FromUnixTimeSeconds(long.Parse(expiry)).UtcDateTime;
        bool expired = expDateTime < DateTime.UtcNow;
        
        Logger.Log($"Token expired: {expired} (expiry: {expDateTime})");
        
        return expired;
    }
    
    #endregion
}