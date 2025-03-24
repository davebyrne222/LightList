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

public class AuthService: IAuthService
{
    private AmazonCognitoIdentityProviderClient? _provider;
    private readonly ISecureStorageRepository _secureStorage;

    public AuthService(ISecureStorageRepository repository)
    {
        Logger.Log("Initializing");
        _secureStorage = repository;
    }

    private AmazonCognitoIdentityProviderClient Provider => _provider ??=
        new AmazonCognitoIdentityProviderClient(new AnonymousAWSCredentials(), Constants.AwsRegion);

    /**
     * Authorisation via AWS Cognito
     *
     * 1. redirect user to Managed Login and retrieve auth code.
     * 2. exchange auth code for JWTs
     */
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
        catch (TaskCanceledException e)
        {
            // User has cancelled login process; do nothing
        }
        
        return false;
    }

    private async Task<bool> ExchangeCodeForTokensAsync(string authCode)
    {
        Logger.Log("Exchanging auth code for JWTs");
        
        var client = new HttpClient();
        var requestBody = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            { "grant_type", "authorization_code" },
            { "client_id", Constants.CognitoAppClientId },
            { "code", authCode },
            { "redirect_uri", Constants.AuthRedirectUrl }
        });

        var response = await client.PostAsync(Constants.CognitoTokenExchangeUrl, requestBody);
        var responseContent = await response.Content.ReadAsStringAsync();
        
        Logger.Log($"Exchange successful: {response.IsSuccessStatusCode}");
        
        if (response.IsSuccessStatusCode)
        {
            try
            {
                AuthTokens? tokens = JsonSerializer.Deserialize<AuthTokens>(responseContent);

                Logger.Log($"Decoded tokens successfully: {tokens != null}");

                if (tokens != null)
                {
                    await _secureStorage.SaveAuthTokensAsync(tokens);
                    return true;
                }
            }
            catch (Exception ex)
            {
                Logger.Log($"Error storing access tokens: {ex.Message}");
            }
        }
        
        return false;
    }

    internal async Task<AuthTokens?> GetAuthTokensAsync()
    {
        Logger.Log("Getting auth tokens");
        return await _secureStorage.GetAuthTokensAsync();
    }
    
    public void SignOutAsync()
    {
        // TODO: Manage properly with cognito
        Logger.Log("Signing out");
        _secureStorage.DeleteAuthTokens();
    }

    public async Task<bool> IsUserLoggedIn()
    {
        // TODO: do properly - check if user logged in
        var token = await SecureStorage.Default.GetAsync("id_token");
        return !string.IsNullOrEmpty(token);
    }
}