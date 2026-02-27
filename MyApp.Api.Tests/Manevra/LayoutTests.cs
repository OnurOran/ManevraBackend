using System.Net;
using System.Net.Http.Json;
using MyApp.Api.Tests.Helpers;

namespace MyApp.Api.Tests.Manevra;

[Collection("Api")]
public class LayoutTests
{
    private readonly ApiWebApplicationFactory _factory;

    public LayoutTests(ApiWebApplicationFactory factory) => _factory = factory;

    [Fact]
    public async Task GetLayout_WithAuth_Returns200WithTracks()
    {
        var client = await _factory.CreateAuthenticatedClientAsync();

        var response = await client.GetAsync("/api/v1/manevra/layout");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<ApiResponse<LayoutResponse>>();
        Assert.True(body!.Success);
        Assert.NotNull(body.Data);
        Assert.NotEmpty(body.Data.Tracks);
    }

    [Fact]
    public async Task GetLayout_ContainsAllZones()
    {
        var client = await _factory.CreateAuthenticatedClientAsync();

        var body = await client.GetFromJsonAsync<ApiResponse<LayoutResponse>>("/api/v1/manevra/layout");

        var zones = body!.Data!.Tracks.Select(t => t.Zone).Distinct().OrderBy(z => z).ToList();
        Assert.Contains((byte)1, zones); // Garaj
        Assert.Contains((byte)2, zones); // Atolye
        Assert.Contains((byte)3, zones); // CariHattaHazirDiziler
    }

    [Fact]
    public async Task GetLayout_GarajTracksHaveCorrectSlotCounts()
    {
        var client = await _factory.CreateAuthenticatedClientAsync();

        var body = await client.GetFromJsonAsync<ApiResponse<LayoutResponse>>("/api/v1/manevra/layout");

        // G1 should have 4 BasMakas + 4 Itfaiye = 8 slots
        var g1 = body!.Data!.Tracks.First(t => t.Name == "G1");
        Assert.Equal(8, g1.Slots.Count);

        // G13 should have 4 BasMakas + 8 Itfaiye = 12 slots
        var g13 = body.Data.Tracks.First(t => t.Name == "G13");
        Assert.Equal(12, g13.Slots.Count);
    }

    [Fact]
    public async Task GetLayout_Zone3Has26Tracks()
    {
        var client = await _factory.CreateAuthenticatedClientAsync();

        var body = await client.GetFromJsonAsync<ApiResponse<LayoutResponse>>("/api/v1/manevra/layout");

        var zone3Tracks = body!.Data!.Tracks.Where(t => t.Zone == 3).ToList();
        Assert.Equal(26, zone3Tracks.Count);
        Assert.All(zone3Tracks, t => Assert.Equal(4, t.Slots.Count));
    }

    [Fact]
    public async Task GetLayout_WithoutAuth_Returns401()
    {
        var client = _factory.CreateClient();

        var response = await client.GetAsync("/api/v1/manevra/layout");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
}
