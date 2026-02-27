using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;

namespace MyApp.Api.Tests.Auth;

[Collection("Api")]
public class LoginTests(ApiWebApplicationFactory factory)
{
    private HttpClient CreateClient() => factory.CreateClient(new WebApplicationFactoryClientOptions
    {
        HandleCookies = true,
    });

    [Fact]
    public async Task Login_WithValidCredentials_Returns200WithAccessToken()
    {
        var client = CreateClient();

        var response = await client.PostAsJsonAsync("/api/v1/users/login", new
        {
            email = ApiWebApplicationFactory.AdminEmail,
            password = ApiWebApplicationFactory.AdminPassword,
        });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var body = await response.Content.ReadFromJsonAsync<ApiResponse<AuthResponse>>();
        Assert.NotNull(body);
        Assert.True(body.Success);
        Assert.NotNull(body.Data);
        Assert.False(string.IsNullOrEmpty(body.Data.AccessToken));
        Assert.True(body.Data.AccessTokenExpiresAt > DateTime.UtcNow);
    }

    [Fact]
    public async Task Login_WithValidCredentials_SetsRefreshTokenCookie()
    {
        var client = CreateClient();

        var response = await client.PostAsJsonAsync("/api/v1/users/login", new
        {
            email = ApiWebApplicationFactory.AdminEmail,
            password = ApiWebApplicationFactory.AdminPassword,
        });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var setCookieHeader = response.Headers.GetValues("Set-Cookie");
        Assert.Contains(setCookieHeader, c => c.StartsWith("refreshToken="));
    }

    [Fact]
    public async Task Login_WithWrongPassword_Returns401()
    {
        var client = CreateClient();

        var response = await client.PostAsJsonAsync("/api/v1/users/login", new
        {
            email = ApiWebApplicationFactory.AdminEmail,
            password = "wrong-password",
        });

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Login_WithUnknownEmail_Returns401()
    {
        var client = CreateClient();

        var response = await client.PostAsJsonAsync("/api/v1/users/login", new
        {
            email = "nobody@example.com",
            password = "any-password",
        });

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Login_WithEmptyBody_Returns400()
    {
        var client = CreateClient();

        var response = await client.PostAsJsonAsync("/api/v1/users/login", new { });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Login_AfterTooManyFailedAttempts_ReturnsLockedOut()
    {
        var client = CreateClient();

        // Register a dedicated user — do not reuse the shared admin account,
        // as locking it out would break other tests in this collection.
        var email = $"lockout-{Guid.NewGuid()}@example.com";
        var register = await client.PostAsJsonAsync("/api/v1/users/register", new
        {
            email,
            password = "Valid123!",
            firstName = "Lock",
            lastName = "Test",
        });
        register.EnsureSuccessStatusCode();

        // MaxFailedAccessAttempts = 5; five wrong attempts should trigger the lockout.
        for (var i = 0; i < 5; i++)
        {
            await client.PostAsJsonAsync("/api/v1/users/login", new
            {
                email,
                password = "wrong-password",
            });
        }

        // The 6th attempt must be rejected with a lockout message.
        var response = await client.PostAsJsonAsync("/api/v1/users/login", new
        {
            email,
            password = "wrong-password",
        });

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);

        var body = await response.Content.ReadFromJsonAsync<ApiResponse<AuthResponse>>();
        Assert.NotNull(body);
        Assert.False(body.Success);
        Assert.NotNull(body.Errors);
        Assert.Contains(body.Errors, e => e.Contains("locked", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public async Task Login_ResponseDoesNotContainRefreshTokenInBody()
    {
        var client = CreateClient();

        var response = await client.PostAsJsonAsync("/api/v1/users/login", new
        {
            email = ApiWebApplicationFactory.AdminEmail,
            password = ApiWebApplicationFactory.AdminPassword,
        });

        var json = await response.Content.ReadAsStringAsync();

        // RefreshToken is [JsonIgnore] on AuthResponse — must never appear in the body.
        Assert.DoesNotContain("refreshToken", json, StringComparison.OrdinalIgnoreCase);
    }
}
