using MyApp.Api.Common.Authorization;
using MyApp.Api.Common.Behaviors;
using MyApp.Api.Common.Extensions;
using MyApp.Api.Common.Models;
using MyApp.Api.Contracts.Manevra;
using MyApp.Api.Common.Attributes;

namespace MyApp.Api.Features.FaultyWagons.GetFaultyWagons;

[MapToGroup("faulty-wagons")]
public static class GetFaultyWagonsEndpoint
{
    public static void Map(RouteGroupBuilder group)
    {
        group.MapGet("/", async Task<IResult> (
            IQueryHandler<GetFaultyWagonsQuery, List<FaultyWagonResponse>> handler,
            CancellationToken ct) =>
        {
            var result = await handler.Handle(new GetFaultyWagonsQuery(), ct);
            return result.IsSuccess
                ? Results.Ok(ApiResponse<List<FaultyWagonResponse>>.Ok(result.Value!))
                : Results.BadRequest(ApiResponse<List<FaultyWagonResponse>>.Fail(result.Error!));
        })
        .WithName("GetFaultyWagons")
        .WithTags("FaultyWagons")
        .RequirePermission(Permissions.Field.View);
    }
}
