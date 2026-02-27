using System.Net;
using System.Net.Http.Json;
using MyApp.Api.Tests.Helpers;

namespace MyApp.Api.Tests.Manevra;

[Collection("Api")]
public class CleanupTests
{
    private readonly ApiWebApplicationFactory _factory;

    public CleanupTests(ApiWebApplicationFactory factory) => _factory = factory;

    [Fact]
    public async Task GetCleanupList_WithAuth_Returns200()
    {
        var client = await _factory.CreateAuthenticatedClientAsync();

        var response = await client.GetAsync("/api/v1/cleanup");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<ApiResponse<List<CleanupEntryResponse>>>();
        Assert.True(body!.Success);
        Assert.NotNull(body.Data);
    }

    [Fact]
    public async Task AddCleanup_WithValidData_Returns201()
    {
        var client = await _factory.CreateAuthenticatedClientAsync();

        var wagons = await client.GetFromJsonAsync<ApiResponse<List<WagonResponse>>>("/api/v1/wagons");
        var wagonId = wagons!.Data!.First().Id;

        var response = await client.PostAsJsonAsync("/api/v1/cleanup", new AddCleanupRequest
        {
            WagonId = wagonId,
            CleanupDate = DateTime.UtcNow,
        });

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<ApiResponse<CleanupEntryResponse>>();
        Assert.True(body!.Success);
        Assert.Equal(wagonId, body.Data!.WagonId);
    }

    [Fact]
    public async Task GetCleanupList_WithoutAuth_Returns401()
    {
        var client = _factory.CreateClient();

        var response = await client.GetAsync("/api/v1/cleanup");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
}
