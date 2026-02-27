using System.Net;
using System.Net.Http.Json;
using MyApp.Api.Tests.Helpers;

namespace MyApp.Api.Tests.Manevra;

[Collection("Api")]
public class MoveWagonTests
{
    private readonly ApiWebApplicationFactory _factory;

    public MoveWagonTests(ApiWebApplicationFactory factory) => _factory = factory;

    private async Task<(HttpClient Client, LayoutResponse Layout)> SetupAsync()
    {
        var client = await _factory.CreateAuthenticatedClientAsync();
        var body = await client.GetFromJsonAsync<ApiResponse<LayoutResponse>>("/api/v1/manevra/layout");
        return (client, body!.Data!);
    }

    [Fact]
    public async Task MoveWagon_UnplacedToZone1_Returns200()
    {
        var (client, layout) = await SetupAsync();

        // Find unplaced wagon
        var wagonsRes = await client.GetFromJsonAsync<ApiResponse<List<WagonResponse>>>("/api/v1/wagons");
        var unplaced = wagonsRes!.Data!.First(w => w.SlotId is null);

        // Find empty Zone 1 slot
        var emptySlot = layout.Tracks
            .Where(t => t.Zone == 1)
            .SelectMany(t => t.Slots)
            .First(s => s.Wagon is null);

        var response = await client.PostAsJsonAsync("/api/v1/manevra/move", new MoveWagonRequest
        {
            WagonId = unplaced.Id,
            TargetSlotId = emptySlot.Id,
        });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<ApiResponse<object>>();
        Assert.True(body!.Success);
    }

    [Fact]
    public async Task MoveWagon_ToOccupiedSlot_Returns400()
    {
        var (client, layout) = await SetupAsync();

        // First, place a wagon
        var wagonsRes = await client.GetFromJsonAsync<ApiResponse<List<WagonResponse>>>("/api/v1/wagons");
        var wagon1 = wagonsRes!.Data!.First(w => w.SlotId is null);
        var emptySlot = layout.Tracks
            .Where(t => t.Zone == 1)
            .SelectMany(t => t.Slots)
            .First(s => s.Wagon is null);

        await client.PostAsJsonAsync("/api/v1/manevra/move", new MoveWagonRequest
        {
            WagonId = wagon1.Id,
            TargetSlotId = emptySlot.Id,
        });

        // Try to place another wagon in the same slot
        var wagon2 = wagonsRes.Data!.First(w => w.SlotId is null && w.Id != wagon1.Id);
        var response = await client.PostAsJsonAsync("/api/v1/manevra/move", new MoveWagonRequest
        {
            WagonId = wagon2.Id,
            TargetSlotId = emptySlot.Id,
        });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task MoveWagon_TramvayToZone3_Returns400()
    {
        var (client, layout) = await SetupAsync();

        // Find a Tramvay wagon
        var wagonsRes = await client.GetFromJsonAsync<ApiResponse<List<WagonResponse>>>("/api/v1/wagons?line=2");
        var tramvay = wagonsRes!.Data!.First(w => w.SlotId is null);

        // Find Zone 3 slot
        var zone3Slot = layout.Tracks
            .Where(t => t.Zone == 3)
            .SelectMany(t => t.Slots)
            .First(s => s.Wagon is null);

        var response = await client.PostAsJsonAsync("/api/v1/manevra/move", new MoveWagonRequest
        {
            WagonId = tramvay.Id,
            TargetSlotId = zone3Slot.Id,
        });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task MoveWagon_UnplacedDirectlyToZone3_Returns400()
    {
        var (client, layout) = await SetupAsync();

        // Find M1 unplaced wagon
        var wagonsRes = await client.GetFromJsonAsync<ApiResponse<List<WagonResponse>>>("/api/v1/wagons?line=1");
        var wagon = wagonsRes!.Data!.First(w => w.SlotId is null);

        // Find Zone 3 slot
        var zone3Slot = layout.Tracks
            .Where(t => t.Zone == 3)
            .SelectMany(t => t.Slots)
            .First(s => s.Wagon is null);

        // Should fail because wagon must be in Zone 1/2 first
        var response = await client.PostAsJsonAsync("/api/v1/manevra/move", new MoveWagonRequest
        {
            WagonId = wagon.Id,
            TargetSlotId = zone3Slot.Id,
        });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
}
