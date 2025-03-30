using System.Text.Json.Serialization;

namespace LightList.Models;

public class AuthTokens
{
    [JsonPropertyName("access_token")]
    public required string AccessToken { get; set; }

    [JsonPropertyName("id_token")]
    public required string IdToken { get; set; }

    [JsonPropertyName("refresh_token")]
    public required string RefreshToken { get; set; }
    
    public string UserId { get; set; } = string.Empty;
}