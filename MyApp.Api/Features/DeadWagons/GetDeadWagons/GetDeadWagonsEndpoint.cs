using MyApp.Api.Common.Authorization;
using MyApp.Api.Common.Behaviors;
using MyApp.Api.Common.Extensions;
using MyApp.Api.Common.Models;
using MyApp.Api.Contracts.Manevra;
using MyApp.Api.Common.Attributes;

namespace MyApp.Api.Features.DeadWagons.GetDeadWagons;

[MapToGroup("dead-wagons")]
public static class GetDeadWagonsEndpoint
{
    public static void Map(RouteGroupBuilder group)
    {
        group.MapGet("/", async Task<IResult> (
            IQueryHandler<GetDeadWagonsQuery, List<DeadWagonResponse>> handler,
            CancellationToken ct) =>
        {
            var result = await handler.Handle(new GetDeadWagonsQuery(), ct);
            return result.IsSuccess
                ? Results.Ok(ApiResponse<List<DeadWagonResponse>>.Ok(result.Value!))
                : Results.BadRequest(ApiResponse<List<DeadWagonResponse>>.Fail(result.Error!));
        })
        .WithName("GetDeadWagons")
        .WithTags("DeadWagons")
        .RequirePermission(Permissions.Manevra.View);
    }
}
