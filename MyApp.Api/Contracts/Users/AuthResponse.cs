using System.Text.Json.Serialization;

namespace MyApp.Api.Contracts.Users;

public class AuthResponse
{
    public string AccessToken { get; set; } = string.Empty;

    // Never serialized to the client — carried internally from handler to endpoint
    // so the endpoint can write it into the HttpOnly cookie.
    [JsonIgnore]
    public string RefreshToken { get; set; } = string.Empty;

    public DateTime AccessTokenExpiresAt { get; set; }
}
