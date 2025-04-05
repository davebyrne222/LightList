using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Security.Authentication;
using System.Text.Json;
using Amazon.CognitoIdentityProvider;
using Amazon.CognitoIdentityProvider.Model;
using Amazon.Runtime;
using LightList.Models;
using LightList.Repositories;
using LightList.Utils;

namespace LightList.Services;

public class AuthService : IAuthService
{
    private readonly ILogger _logger;
    private readonly ISecureStorageRepository _secureStorage;
    private AmazonCognitoIdentityProviderClient? _provider;

    public AuthService(ILogger logger, ISecureStorageRepository repository)
    {
        _logger = logger;
        _logger.Debug("Initializing");
        _secureStorage = repository;
        _logger.Debug("Initialized");
    }

    private AmazonCognitoIdentityProviderClient Provider => _provider ??=
        new AmazonCognitoIdentityProviderClient(new AnonymousAWSCredentials(), Constants.AwsRegion);

    #region Public Methods

    public async Task<bool> SignInAsync()
    {
        _logger.Debug($"Redirecting to Cognito Auth: {new Uri(Constants.CognitoAuthUrl)}");

        try
        {
            var authResult = await WebAuthenticator.Default.AuthenticateAsync(
                new Uri(Constants.CognitoAuthUrl),
                new Uri(Constants.AuthRedirectUrl)
            );

            _logger.Debug($"Authenticated: {authResult.Properties["code"] != null}");

            if (authResult.Properties["code"] != null)
                return await ExchangeCodeForTokensAsync(authResult.Properties["code"]);
        }
        catch (TaskCanceledException)
        {
            // User has cancelled login process; do nothing
        }
        catch (Exception ex)
        {
            _logger.Error($"Exception while signing in: {ex.GetType().FullName} - {ex.Message}");
        }

        return false;
    }

    public async Task<bool> SignOutAsync()
    {
        // Sign out from cognito
        _logger.Debug("Signing out: requesting cognito sign out");

        var tokens = await _secureStorage.GetAuthTokensAsync();

        if (tokens == null)
            return true;

        var request = new GlobalSignOutRequest
        {
            AccessToken = tokens.AccessToken
        };

        try
        {
            await Provider.GlobalSignOutAsync(request);
            //TODO: redirect to /logout? https://docs.aws.amazon.com/cognito/latest/developerguide/amazon-cognito-user-pools-using-the-refresh-token.html
        }
        catch (Exception ex)
        {
            _logger.Error($"Error signing out from cognito: {ex.GetType().FullName} - {ex.Message}");
        }

        // Delete stored tokens
        _secureStorage.DeleteAuthTokens();

        return true;
    }

    public async Task<bool> IsUserLoggedIn()
    {
        _logger.Debug("Checking if user is logged in");

        var authTokens = await _secureStorage.GetAuthTokensAsync();

        if (authTokens == null)
        {
            _logger.Debug("No auth tokens found");
            return false;
        }

        // Check if access token is valid - if it is, user is logged in
        if (!IsTokenExpired(authTokens.AccessToken))
        {
            _logger.Debug("Token valid. User is logged in");
            return true;
        }

        // If access token expired, refresh
        _logger.Debug("Token expired. Requesting refresh");
        var refreshed = await RefreshAccessTokenAsync(authTokens);

        _logger.Debug($"Access token refreshed: {refreshed}");
        return refreshed;
    }

    #endregion

    #region Utils

    private async Task<bool> ExchangeCodeForTokensAsync(string authCode)
    {
        _logger.Debug("Exchanging auth code for JWTs");

        // Exchange code
        var client = new HttpClient();
        var requestBody = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            { "grant_type", "authorization_code" },
            { "client_id", Constants.CognitoAppClientId },
            { "code", authCode },
            { "redirect_uri", Constants.AuthRedirectUrl }
        });

        var response = await client.PostAsync(Constants.CognitoTokenExchangeUrl, requestBody);

        if (!response.IsSuccessStatusCode)
        {
            _logger.Debug($"Exchange failed: {response.StatusCode} - {response.ReasonPhrase}");
            return false;
        }

        _logger.Debug("Exchange successful. Saving tokens");

        // save tokens
        try
        {
            var authString = await response.Content.ReadAsStringAsync();
            return await _secureStorage.SaveAuthTokensAsync(authString);
        }
        catch (Exception ex)
        {
            _logger.Error($"Error storing access tokens: {ex.Message}");
            throw;
        }
    }

    private async Task<bool> RefreshAccessTokenAsync(AuthTokens authTokens)
    {
        _logger.Debug("Refreshing access token");

        // Request new token
        var request = new InitiateAuthRequest
        {
            AuthFlow = AuthFlowType.REFRESH_TOKEN_AUTH,
            ClientId = Constants.CognitoAppClientId,
            AuthParameters = new Dictionary<string, string> { { "REFRESH_TOKEN", authTokens.RefreshToken } }
        };

        InitiateAuthResponse response;

        try
        {
            response = await Provider.InitiateAuthAsync(request);
        }
        catch (Exception ex)
        {
            _logger.Error($"Error refreshing auth tokens: {ex.GetType().FullName} - {ex.Message}");
            throw;
        }

        if (response.HttpStatusCode != HttpStatusCode.OK)
            throw new AuthenticationException(
                $"Failed to refresh access token. HTTP Status: {response.HttpStatusCode} (Challenge: {response.ChallengeName})");

        if (response.AuthenticationResult == null)
            throw new AuthenticationException("Failed to refresh access token. AuthenticationResult == null");

        // Save new token
        _logger.Debug("Token refreshed. Saving");

        try
        {
            authTokens.AccessToken = response.AuthenticationResult.AccessToken;
            var tokenString = JsonSerializer.Serialize(authTokens);
            return await _secureStorage.SaveAuthTokensAsync(tokenString);
        }
        catch (Exception ex)
        {
            _logger.Error($"Error storing access tokens: {ex.Message}");
            throw;
        }
    }

    private bool IsTokenExpired(string token)
    {
        _logger.Debug("Checking if access token is expired");

        // Get 'exp' claim
        JwtSecurityTokenHandler handler = new();
        var jwtToken = handler.ReadJwtToken(token);
        var expiry = jwtToken.Claims.FirstOrDefault(c => c.Type == "exp")?.Value;

        // If no 'exp' claim, token is _likely_ valid (may not be on auth server)
        if (string.IsNullOrEmpty(expiry))
        {
            _logger.Debug("No expiry claim found");
            return false;
        }

        // Check if exp has passed
        var expDateTime = DateTimeOffset.FromUnixTimeSeconds(long.Parse(expiry)).UtcDateTime;
        var expired = expDateTime < DateTime.UtcNow;

        _logger.Debug($"Token expired: {expDateTime} < {DateTime.UtcNow}: {expired}");

        return expired;
    }

    #endregion
}