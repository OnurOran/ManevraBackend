using System.Net;
using System.Net.Http.Json;
using MyApp.Api.Tests.Helpers;

namespace MyApp.Api.Tests.Users;

[Collection("Api")]
public class GetUsersTests(ApiWebApplicationFactory factory)
{
    [Fact]
    public async Task GetUsers_WithAdminToken_Returns200WithPagedResult()
    {
        var client = await factory.CreateAuthenticatedClientAsync();

        var response = await client.GetAsync("/api/v1/users?page=1&pageSize=20");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var body = await response.Content.ReadFromJsonAsync<ApiResponse<PagedResult<object>>>();
        Assert.NotNull(body);
        Assert.True(body.Success);
        Assert.NotNull(body.Data);
        Assert.True(body.Data.TotalCount >= 1); // at least the seeded admin
        Assert.True(body.Data.TotalPages >= 1);
    }

    [Fact]
    public async Task GetUsers_WithoutToken_Returns401()
    {
        var client = factory.CreateClient();

        var response = await client.GetAsync("/api/v1/users");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task GetUserById_WithAdminToken_Returns200()
    {
        // First get the users list to find the admin's id.
        var client = await factory.CreateAuthenticatedClientAsync();

        var listResponse = await client.GetAsync("/api/v1/users?page=1&pageSize=20");
        listResponse.EnsureSuccessStatusCode();

        var list = await listResponse.Content.ReadFromJsonAsync<ApiResponse<PagedResult<UserListItem>>>();
        var adminUser = list!.Data!.Items.FirstOrDefault(u => u.Email == ApiWebApplicationFactory.AdminEmail);
        Assert.NotNull(adminUser);

        var byIdResponse = await client.GetAsync($"/api/v1/users/{adminUser.Id}");

        Assert.Equal(HttpStatusCode.OK, byIdResponse.StatusCode);
    }

    // Minimal DTO for deserialising the users list — only fields needed by tests.
    private record UserListItem(Guid Id, string Email);
}
