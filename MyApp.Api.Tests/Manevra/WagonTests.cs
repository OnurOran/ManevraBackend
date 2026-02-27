using System.Net;
using System.Net.Http.Json;
using MyApp.Api.Tests.Helpers;

namespace MyApp.Api.Tests.Manevra;

[Collection("Api")]
public class WagonTests
{
    private readonly ApiWebApplicationFactory _factory;

    public WagonTests(ApiWebApplicationFactory factory) => _factory = factory;

    [Fact]
    public async Task GetWagons_WithAuth_Returns200()
    {
        var client = await _factory.CreateAuthenticatedClientAsync();

        var response = await client.GetAsync("/api/v1/wagons");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<ApiResponse<List<WagonResponse>>>();
        Assert.True(body!.Success);
        Assert.NotNull(body.Data);
        Assert.NotEmpty(body.Data);
    }

    [Fact]
    public async Task GetWagons_FilterByLine_ReturnsFiltered()
    {
        var client = await _factory.CreateAuthenticatedClientAsync();

        var response = await client.GetAsync("/api/v1/wagons?line=1");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<ApiResponse<List<WagonResponse>>>();
        Assert.True(body!.Success);
        Assert.All(body.Data!, w => Assert.Equal(1, w.Line));
    }

    [Fact]
    public async Task GetWagons_WithoutAuth_Returns401()
    {
        var client = _factory.CreateClient();

        var response = await client.GetAsync("/api/v1/wagons");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task CreateWagon_WithValidData_Returns201()
    {
        var client = await _factory.CreateAuthenticatedClientAsync();

        var response = await client.PostAsJsonAsync("/api/v1/wagons", new CreateWagonRequest
        {
            WagonNumber = 199,
            Line = 1,
        });

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<ApiResponse<WagonResponse>>();
        Assert.True(body!.Success);
        Assert.Equal(199, body.Data!.WagonNumber);
        Assert.True(body.Data.IsOnlyMiddle); // starts with 1 → IsOnlyMiddle=TRUE
    }

    [Fact]
    public async Task CreateWagon_Series5_HasIsOnlyMiddleFalse()
    {
        var client = await _factory.CreateAuthenticatedClientAsync();

        var response = await client.PostAsJsonAsync("/api/v1/wagons", new CreateWagonRequest
        {
            WagonNumber = 599,
            Line = 1,
        });

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<ApiResponse<WagonResponse>>();
        Assert.False(body!.Data!.IsOnlyMiddle); // starts with 5 → IsOnlyMiddle=FALSE
    }

    [Fact]
    public async Task UpdateWagonStatus_ValidStatus_Returns200()
    {
        var client = await _factory.CreateAuthenticatedClientAsync();

        // Get a wagon
        var wagonsRes = await client.GetFromJsonAsync<ApiResponse<List<WagonResponse>>>("/api/v1/wagons");
        var wagonId = wagonsRes!.Data!.First().Id;

        var response = await client.PutAsJsonAsync($"/api/v1/wagons/{wagonId}/status", new UpdateWagonStatusRequest
        {
            Status = 2, // CalismaYapilacak
        });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task ToggleWagonMiddle_Returns200()
    {
        var client = await _factory.CreateAuthenticatedClientAsync();

        // Find a 5xx wagon
        var wagonsRes = await client.GetFromJsonAsync<ApiResponse<List<WagonResponse>>>("/api/v1/wagons");
        var wagon = wagonsRes!.Data!.First(w => w.WagonNumber.ToString().StartsWith('5'));
        var originalMiddle = wagon.IsOnlyMiddle;

        var response = await client.PutAsync($"/api/v1/wagons/{wagon.Id}/toggle-middle", null);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        // Verify toggled
        var updated = await client.GetFromJsonAsync<ApiResponse<List<WagonResponse>>>("/api/v1/wagons");
        var updatedWagon = updated!.Data!.First(w => w.Id == wagon.Id);
        Assert.NotEqual(originalMiddle, updatedWagon.IsOnlyMiddle);
    }
}
