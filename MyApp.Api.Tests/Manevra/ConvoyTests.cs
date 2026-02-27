using System.Net;
using System.Net.Http.Json;
using MyApp.Api.Tests.Helpers;

namespace MyApp.Api.Tests.Manevra;

[Collection("Api")]
public class ConvoyTests
{
    private readonly ApiWebApplicationFactory _factory;

    public ConvoyTests(ApiWebApplicationFactory factory) => _factory = factory;

    private async Task<int> PlaceWagonInZone1Async(HttpClient client, int wagonId)
    {
        var layout = await client.GetFromJsonAsync<ApiResponse<LayoutResponse>>("/api/v1/manevra/layout");
        var emptySlot = layout!.Data!.Tracks
            .Where(t => t.Zone == 1)
            .SelectMany(t => t.Slots)
            .First(s => s.Wagon is null);

        await client.PostAsJsonAsync("/api/v1/manevra/move", new MoveWagonRequest
        {
            WagonId = wagonId,
            TargetSlotId = emptySlot.Id,
        });
        return emptySlot.Id;
    }

    [Fact]
    public async Task CreateConvoy_WithTwoWagons_ReturnsConvoyId()
    {
        var client = await _factory.CreateAuthenticatedClientAsync();
        var wagons = await client.GetFromJsonAsync<ApiResponse<List<WagonResponse>>>("/api/v1/wagons");
        var unplaced = wagons!.Data!.Where(w => w.SlotId is null && w.ConvoyId is null).Take(2).ToList();

        var response = await client.PostAsJsonAsync("/api/v1/manevra/convoy", new CreateConvoyRequest
        {
            WagonIds = unplaced.Select(w => w.Id).ToList(),
        });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<ApiResponse<Guid>>();
        Assert.True(body!.Success);
        Assert.NotEqual(Guid.Empty, body.Data);
    }

    [Fact]
    public async Task CreateConvoy_WithOneWagon_Returns400()
    {
        var client = await _factory.CreateAuthenticatedClientAsync();
        var wagons = await client.GetFromJsonAsync<ApiResponse<List<WagonResponse>>>("/api/v1/wagons");
        var unplaced = wagons!.Data!.First(w => w.SlotId is null && w.ConvoyId is null);

        var response = await client.PostAsJsonAsync("/api/v1/manevra/convoy", new CreateConvoyRequest
        {
            WagonIds = [unplaced.Id],
        });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task DetachFromConvoy_RemovesWagonFromConvoy()
    {
        var client = await _factory.CreateAuthenticatedClientAsync();
        var wagons = await client.GetFromJsonAsync<ApiResponse<List<WagonResponse>>>("/api/v1/wagons");
        var unplaced = wagons!.Data!.Where(w => w.SlotId is null && w.ConvoyId is null).Take(3).ToList();

        // Create convoy with 3 wagons
        var createRes = await client.PostAsJsonAsync("/api/v1/manevra/convoy", new CreateConvoyRequest
        {
            WagonIds = unplaced.Select(w => w.Id).ToList(),
        });
        createRes.EnsureSuccessStatusCode();

        // Detach first wagon
        var response = await client.PostAsJsonAsync("/api/v1/manevra/convoy/detach", new { wagonId = unplaced[0].Id });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task DisbandConvoy_RemovesAllWagonsFromConvoy()
    {
        var client = await _factory.CreateAuthenticatedClientAsync();
        var wagons = await client.GetFromJsonAsync<ApiResponse<List<WagonResponse>>>("/api/v1/wagons");
        var unplaced = wagons!.Data!.Where(w => w.SlotId is null && w.ConvoyId is null).Take(2).ToList();

        var createRes = await client.PostAsJsonAsync("/api/v1/manevra/convoy", new CreateConvoyRequest
        {
            WagonIds = unplaced.Select(w => w.Id).ToList(),
        });
        var convoyBody = await createRes.Content.ReadFromJsonAsync<ApiResponse<Guid>>();
        var convoyId = convoyBody!.Data;

        var response = await client.DeleteAsync($"/api/v1/manevra/convoy/{convoyId}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }
}
