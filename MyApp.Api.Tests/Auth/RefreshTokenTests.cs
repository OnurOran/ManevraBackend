using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;

namespace MyApp.Api.Tests.Auth;

[Collection("Api")]
public class RefreshTokenTests(ApiWebApplicationFactory factory)
{
    private HttpClient CreateClient() => factory.CreateClient(new WebApplicationFactoryClientOptions
    {
        // Cookie handling is required: the refresh token flows via Set-Cookie / Cookie headers.
        HandleCookies = true,
    });

    [Fact]
    public async Task RefreshToken_AfterLogin_Returns200WithNewAccessToken()
    {
        var client = CreateClient();

        // Step 1 — login; the handler sets the refreshToken cookie automatically.
        var loginResponse = await client.PostAsJsonAsync("/api/v1/users/login", new
        {
            email = ApiWebApplicationFactory.AdminEmail,
            password = ApiWebApplicationFactory.AdminPassword,
        });
        loginResponse.EnsureSuccessStatusCode();
        var loginBody = await loginResponse.Content.ReadFromJsonAsync<ApiResponse<AuthResponse>>();
        var firstToken = loginBody!.Data!.AccessToken;

        // Step 2 — refresh; the cookie is sent back automatically by the HttpClient.
        var refreshResponse = await client.PostAsync("/api/v1/users/refresh-token", null);

        Assert.Equal(HttpStatusCode.OK, refreshResponse.StatusCode);

        var body = await refreshResponse.Content.ReadFromJsonAsync<ApiResponse<AuthResponse>>();
        Assert.NotNull(body);
        Assert.True(body.Success);
        Assert.NotNull(body.Data);
        Assert.False(string.IsNullOrEmpty(body.Data.AccessToken));

        // The new token is a valid JWT (three dot-separated segments).
        Assert.Equal(3, body.Data.AccessToken.Split('.').Length);

        // A new refresh token cookie is set (rotation).
        var setCookieHeader = refreshResponse.Headers.GetValues("Set-Cookie");
        Assert.Contains(setCookieHeader, c => c.StartsWith("refreshToken="));

        _ = firstToken; // suppress unused warning — kept for readability
    }

    [Fact]
    public async Task RefreshToken_WithNoCookie_Returns401()
    {
        // Client with no prior login — no refresh token cookie present.
        var client = CreateClient();

        var response = await client.PostAsync("/api/v1/users/refresh-token", null);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task RefreshToken_AfterLogout_Returns401()
    {
        var client = CreateClient();

        // Login to get a valid cookie.
        var loginResponse = await client.PostAsJsonAsync("/api/v1/users/login", new
        {
            email = ApiWebApplicationFactory.AdminEmail,
            password = ApiWebApplicationFactory.AdminPassword,
        });
        loginResponse.EnsureSuccessStatusCode();
        var token = (await loginResponse.Content.ReadFromJsonAsync<ApiResponse<AuthResponse>>())!.Data!.AccessToken;

        // Logout — the backend revokes the token in the DB and clears the cookie.
        client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
        var logoutResponse = await client.PostAsync("/api/v1/users/logout", null);
        logoutResponse.EnsureSuccessStatusCode();

        // Attempting to refresh after logout must fail.
        client.DefaultRequestHeaders.Authorization = null;
        var refreshResponse = await client.PostAsync("/api/v1/users/refresh-token", null);

        Assert.Equal(HttpStatusCode.Unauthorized, refreshResponse.StatusCode);
    }
}
