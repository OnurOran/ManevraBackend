using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;

namespace MyApp.Api.Tests.Helpers;

public static class AuthHelpers
{
    /// <summary>
    /// Logs in with the seeded admin credentials and returns a bearer token.
    /// Call this at the start of any test that requires an authenticated request.
    /// </summary>
    public static async Task<string> GetAdminTokenAsync(this HttpClient client)
    {
        var response = await client.PostAsJsonAsync("/api/v1/users/login", new
        {
            email = ApiWebApplicationFactory.AdminEmail,
            password = ApiWebApplicationFactory.AdminPassword,
        });

        response.EnsureSuccessStatusCode();

        var body = await response.Content.ReadFromJsonAsync<ApiResponse<AuthResponse>>();
        return body!.Data!.AccessToken;
    }

    /// <summary>
    /// Creates a new HttpClient with cookie handling enabled and the Authorization
    /// header pre-set to the admin bearer token.
    /// Use this for tests that call protected endpoints.
    /// </summary>
    public static async Task<HttpClient> CreateAuthenticatedClientAsync(
        this ApiWebApplicationFactory factory)
    {
        var client = factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            HandleCookies = true,
        });

        var token = await client.GetAdminTokenAsync();
        client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        return client;
    }
}
