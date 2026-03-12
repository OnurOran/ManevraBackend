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

    private static List<WagonResponse> GetUnplacedM1Wagons(List<WagonResponse> wagons, int count)
        => wagons.Where(w => w.SlotId is null && w.ConvoyId is null && w.Line == 1).Take(count).ToList();

    private static TrackSlotResponse GetEmptyZone1Slot(LayoutResponse layout)
        => layout.Tracks
            .Where(t => t.Zone == 1)
            .SelectMany(t => t.Slots)
            .First(s => s.Wagon is null);

    private async Task<int> PlaceWagonInZone1(HttpClient client, int wagonId)
    {
        var layout = await client.GetFromJsonAsync<ApiResponse<LayoutResponse>>("/api/v1/manevra/layout");
        var slot = layout!.Data!.Tracks
            .Where(t => t.Zone == 1)
            .SelectMany(t => t.Slots)
            .First(s => s.Wagon is null);

        var res = await client.PostAsJsonAsync("/api/v1/manevra/move", new MoveWagonRequest
        {
            WagonId = wagonId,
            TargetSlotId = slot.Id,
        });
        res.EnsureSuccessStatusCode();
        return slot.Id;
    }

    private async Task SetWagonStatus(HttpClient client, int wagonId, byte status)
    {
        var res = await client.PutAsJsonAsync($"/api/v1/wagons/{wagonId}/status", new { Status = status });
        res.EnsureSuccessStatusCode();
    }

    private async Task<Guid> CreateConvoy(HttpClient client, List<int> wagonIds)
    {
        var res = await client.PostAsJsonAsync("/api/v1/manevra/convoy", new CreateConvoyRequest
        {
            WagonIds = wagonIds,
        });
        res.EnsureSuccessStatusCode();
        var body = await res.Content.ReadFromJsonAsync<ApiResponse<Guid>>();
        return body!.Data;
    }

    // ── Existing tests ───────────────────────────────────────────────────────

    [Fact]
    public async Task MoveWagon_UnplacedToZone1_Returns200()
    {
        var (client, layout) = await SetupAsync();

        var wagonsRes = await client.GetFromJsonAsync<ApiResponse<List<WagonResponse>>>("/api/v1/wagons");
        var unplaced = wagonsRes!.Data!.First(w => w.SlotId is null && w.ConvoyId is null);

        var emptySlot = GetEmptyZone1Slot(layout);

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

        // Filter for non-convoy wagons to avoid convoy move logic
        var wagonsRes = await client.GetFromJsonAsync<ApiResponse<List<WagonResponse>>>("/api/v1/wagons");
        var wagon1 = wagonsRes!.Data!.First(w => w.SlotId is null && w.ConvoyId is null);
        var emptySlot = GetEmptyZone1Slot(layout);

        var moveRes = await client.PostAsJsonAsync("/api/v1/manevra/move", new MoveWagonRequest
        {
            WagonId = wagon1.Id,
            TargetSlotId = emptySlot.Id,
        });
        moveRes.EnsureSuccessStatusCode();

        var wagon2 = wagonsRes.Data!.First(w => w.SlotId is null && w.ConvoyId is null && w.Id != wagon1.Id);
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

        var wagonsRes = await client.GetFromJsonAsync<ApiResponse<List<WagonResponse>>>("/api/v1/wagons?line=2");
        var tramvay = wagonsRes!.Data!.First(w => w.SlotId is null);

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

        var wagonsRes = await client.GetFromJsonAsync<ApiResponse<List<WagonResponse>>>("/api/v1/wagons?line=1");
        var wagon = wagonsRes!.Data!.First(w => w.SlotId is null);

        var zone3Slot = layout.Tracks
            .Where(t => t.Zone == 3)
            .SelectMany(t => t.Slots)
            .First(s => s.Wagon is null);

        var response = await client.PostAsJsonAsync("/api/v1/manevra/move", new MoveWagonRequest
        {
            WagonId = wagon.Id,
            TargetSlotId = zone3Slot.Id,
        });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    // ── Convoy movement tests ────────────────────────────────────────────────

    [Fact]
    public async Task MoveConvoyWagon_MovesAllMembers_Returns200()
    {
        var (client, _) = await SetupAsync();

        // Get two unplaced M1 wagons (5xx series = IsOnlyMiddle=false)
        var wagonsRes = await client.GetFromJsonAsync<ApiResponse<List<WagonResponse>>>("/api/v1/wagons?line=1");
        var candidates = wagonsRes!.Data!
            .Where(w => w.SlotId is null && w.ConvoyId is null && !w.IsOnlyMiddle)
            .Take(2).ToList();
        Assert.True(candidates.Count >= 2, "Need at least 2 unplaced non-IsOnlyMiddle M1 wagons");

        // Place both in Zone 1 on same track section (pick a track with >= 4 empty slots in one section)
        var layout0 = await client.GetFromJsonAsync<ApiResponse<LayoutResponse>>("/api/v1/manevra/layout");
        var sourceTrack = layout0!.Data!.Tracks
            .Where(t => t.Zone == 1)
            .First(t => t.Slots.Count(s => s.Wagon is null) >= 4);
        var sourceSlots = sourceTrack.Slots.Where(s => s.Wagon is null).Take(2).ToList();

        foreach (var (c, s) in candidates.Zip(sourceSlots))
        {
            var res = await client.PostAsJsonAsync("/api/v1/manevra/move", new MoveWagonRequest
            {
                WagonId = c.Id,
                TargetSlotId = s.Id,
            });
            res.EnsureSuccessStatusCode();
        }

        // Create convoy
        await CreateConvoy(client, candidates.Select(w => w.Id).ToList());

        // Find a DIFFERENT Zone 1 track with at least 2 consecutive empty slots in the same section
        var layout = await client.GetFromJsonAsync<ApiResponse<LayoutResponse>>("/api/v1/manevra/layout");
        var targetTrack = layout!.Data!.Tracks
            .Where(t => t.Zone == 1 && t.Id != sourceTrack.Id)
            .First(t => t.Slots.Count(s => s.Wagon is null) >= 2);

        var targetSlot = targetTrack.Slots.First(s => s.Wagon is null);

        // Move one convoy wagon — both should move
        var response = await client.PostAsJsonAsync("/api/v1/manevra/move", new MoveWagonRequest
        {
            WagonId = candidates[0].Id,
            TargetSlotId = targetSlot.Id,
        });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        // Verify both wagons are now on the target track
        var updatedLayout = await client.GetFromJsonAsync<ApiResponse<LayoutResponse>>("/api/v1/manevra/layout");
        var targetTrackSlots = updatedLayout!.Data!.Tracks
            .First(t => t.Id == targetTrack.Id).Slots;

        var placedWagonIds = targetTrackSlots
            .Where(s => s.Wagon is not null)
            .Select(s => s.Wagon!.Id)
            .ToHashSet();

        Assert.Contains(candidates[0].Id, placedWagonIds);
        Assert.Contains(candidates[1].Id, placedWagonIds);
    }

    [Fact]
    public async Task MoveConvoyWagon_NotEnoughSlots_Returns400()
    {
        var (client, _) = await SetupAsync();

        // Get 3 unplaced M1 wagons
        var wagonsRes = await client.GetFromJsonAsync<ApiResponse<List<WagonResponse>>>("/api/v1/wagons?line=1");
        var candidates = wagonsRes!.Data!
            .Where(w => w.SlotId is null && w.ConvoyId is null && !w.IsOnlyMiddle)
            .Take(3).ToList();
        Assert.True(candidates.Count >= 3, "Need at least 3 unplaced non-IsOnlyMiddle M1 wagons");

        // Place all 3 in Zone 1
        foreach (var c in candidates)
            await PlaceWagonInZone1(client, c.Id);

        // Create convoy of 3
        await CreateConvoy(client, candidates.Select(w => w.Id).ToList());

        // Find a Zone 2 track that has AtolyeYollari section with only 1 slot (A7 or A8)
        var layout = await client.GetFromJsonAsync<ApiResponse<LayoutResponse>>("/api/v1/manevra/layout");
        var smallTrack = layout!.Data!.Tracks
            .Where(t => t.Zone == 2)
            .Where(t => t.Slots.Count(s => s.Wagon is null && s.SectionType == 4) == 1) // AtolyeYollari=4
            .FirstOrDefault();
        Assert.NotNull(smallTrack);

        var targetSlot = smallTrack.Slots.First(s => s.Wagon is null && s.SectionType == 4);

        var response = await client.PostAsJsonAsync("/api/v1/manevra/move", new MoveWagonRequest
        {
            WagonId = candidates[0].Id,
            TargetSlotId = targetSlot.Id,
        });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task MoveConvoyToZone3_FirstLastNotOnlyMiddle_Returns200()
    {
        var (client, _) = await SetupAsync();

        // Get two 5xx wagons (IsOnlyMiddle=false)
        var wagonsRes = await client.GetFromJsonAsync<ApiResponse<List<WagonResponse>>>("/api/v1/wagons?line=1");
        var candidates = wagonsRes!.Data!
            .Where(w => w.SlotId is null && w.ConvoyId is null && !w.IsOnlyMiddle)
            .Take(2).ToList();
        Assert.True(candidates.Count >= 2, "Need at least 2 unplaced non-IsOnlyMiddle M1 wagons");

        // Place in Zone 1 and set status to ServiseHazir (3)
        foreach (var c in candidates)
        {
            await PlaceWagonInZone1(client, c.Id);
            await SetWagonStatus(client, c.Id, 3); // ServiseHazir
        }

        // Create convoy
        await CreateConvoy(client, candidates.Select(w => w.Id).ToList());

        // Find Zone 3 track with at least 2 empty slots
        var layout = await client.GetFromJsonAsync<ApiResponse<LayoutResponse>>("/api/v1/manevra/layout");
        var zone3Track = layout!.Data!.Tracks
            .Where(t => t.Zone == 3)
            .FirstOrDefault(t => t.Slots.Count(s => s.Wagon is null) >= 2);
        Assert.NotNull(zone3Track);

        var targetSlot = zone3Track.Slots.First(s => s.Wagon is null);

        var response = await client.PostAsJsonAsync("/api/v1/manevra/move", new MoveWagonRequest
        {
            WagonId = candidates[0].Id,
            TargetSlotId = targetSlot.Id,
        });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task MoveConvoyToZone3_FirstIsOnlyMiddle_Returns400()
    {
        var (client, _) = await SetupAsync();

        // Get one 1xx wagon (IsOnlyMiddle=true) and one 5xx wagon (IsOnlyMiddle=false)
        var wagonsRes = await client.GetFromJsonAsync<ApiResponse<List<WagonResponse>>>("/api/v1/wagons?line=1");
        var middleWagon = wagonsRes!.Data!
            .First(w => w.SlotId is null && w.ConvoyId is null && w.IsOnlyMiddle);
        var endWagon = wagonsRes.Data!
            .First(w => w.SlotId is null && w.ConvoyId is null && !w.IsOnlyMiddle);

        // Place in Zone 1 — place middle wagon first so it gets a lower slotIndex
        await PlaceWagonInZone1(client, middleWagon.Id);
        await PlaceWagonInZone1(client, endWagon.Id);

        // Set both to ServiseHazir
        await SetWagonStatus(client, middleWagon.Id, 3);
        await SetWagonStatus(client, endWagon.Id, 3);

        // Create convoy (middleWagon will be first due to lower slotIndex)
        await CreateConvoy(client, [middleWagon.Id, endWagon.Id]);

        // Find Zone 3 track with at least 2 empty slots
        var layout = await client.GetFromJsonAsync<ApiResponse<LayoutResponse>>("/api/v1/manevra/layout");
        var zone3Track = layout!.Data!.Tracks
            .Where(t => t.Zone == 3)
            .FirstOrDefault(t => t.Slots.Count(s => s.Wagon is null) >= 2);
        Assert.NotNull(zone3Track);

        var targetSlot = zone3Track.Slots.First(s => s.Wagon is null);

        var response = await client.PostAsJsonAsync("/api/v1/manevra/move", new MoveWagonRequest
        {
            WagonId = middleWagon.Id,
            TargetSlotId = targetSlot.Id,
        });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
}
