using System.Net;
using System.Net.Http.Json;

namespace MyApp.Api.Tests.Auth;

[Collection("Api")]
public class RegisterTests(ApiWebApplicationFactory factory)
{
    private HttpClient CreateClient() => factory.CreateClient();

    [Fact]
    public async Task Register_WithValidData_Returns201WithUserDetails()
    {
        var client = CreateClient();
        var email = $"user-{Guid.NewGuid()}@example.com";

        var response = await client.PostAsJsonAsync("/api/v1/users/register", new
        {
            email,
            password = "Test1234!",
            firstName = "Jane",
            lastName = "Doe",
        });

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var body = await response.Content.ReadFromJsonAsync<ApiResponse<UserResponse>>();
        Assert.NotNull(body);
        Assert.True(body.Success);
        Assert.NotNull(body.Data);
        Assert.Equal(email, body.Data.Email);
        Assert.Equal("Jane", body.Data.FirstName);
        Assert.Equal("Doe", body.Data.LastName);
    }

    [Fact]
    public async Task Register_WithDuplicateEmail_Returns400()
    {
        var client = CreateClient();
        var email = $"dup-{Guid.NewGuid()}@example.com";

        var payload = new
        {
            email,
            password = "Test1234!",
            firstName = "A",
            lastName = "B",
        };

        var first = await client.PostAsJsonAsync("/api/v1/users/register", payload);
        first.EnsureSuccessStatusCode();

        var second = await client.PostAsJsonAsync("/api/v1/users/register", payload);

        Assert.Equal(HttpStatusCode.BadRequest, second.StatusCode);
    }

    [Fact]
    public async Task Register_WithWeakPassword_Returns400()
    {
        var client = CreateClient();

        var response = await client.PostAsJsonAsync("/api/v1/users/register", new
        {
            email = $"weak-{Guid.NewGuid()}@example.com",
            password = "abc",     // too short, no digits/symbols
            firstName = "A",
            lastName = "B",
        });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Register_WithMissingFields_Returns400()
    {
        var client = CreateClient();

        var response = await client.PostAsJsonAsync("/api/v1/users/register", new { });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
}
