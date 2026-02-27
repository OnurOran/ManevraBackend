using MyApp.Api.Common.Authorization;
using MyApp.Api.Common.Behaviors;
using MyApp.Api.Common.Extensions;
using MyApp.Api.Common.Models;
using MyApp.Api.Contracts.Manevra;
using MyApp.Api.Common.Attributes;

namespace MyApp.Api.Features.Wagons.GetWagons;

[MapToGroup("wagons")]
public static class GetWagonsEndpoint
{
    public static void Map(RouteGroupBuilder group)
    {
        group.MapGet("/", async Task<IResult> (
            IQueryHandler<GetWagonsQuery, List<WagonResponse>> handler,
            byte? line = null,
            CancellationToken ct = default) =>
        {
            var query = new GetWagonsQuery { Line = line };
            var result = await handler.Handle(query, ct);
            return result.IsSuccess
                ? Results.Ok(ApiResponse<List<WagonResponse>>.Ok(result.Value!))
                : Results.BadRequest(ApiResponse<List<WagonResponse>>.Fail(result.Error!));
        })
        .WithName("GetWagons")
        .WithTags("Wagons")
        .RequirePermission(Permissions.Wagons.View);
    }
}
