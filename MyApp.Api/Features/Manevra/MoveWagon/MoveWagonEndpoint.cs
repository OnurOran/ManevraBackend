using MyApp.Api.Common.Authorization;
using MyApp.Api.Common.Behaviors;
using MyApp.Api.Common.Extensions;
using MyApp.Api.Common.Models;
using MyApp.Api.Contracts.Manevra;
using MyApp.Api.Common.Attributes;

namespace MyApp.Api.Features.Manevra.MoveWagon;

[MapToGroup("manevra")]
public static class MoveWagonEndpoint
{
    public static void Map(RouteGroupBuilder group)
    {
        group.MapPost("/move", async Task<IResult> (
            MoveWagonRequest request,
            ICommandHandler<MoveWagonCommand, bool> handler,
            CancellationToken ct) =>
        {
            var command = new MoveWagonCommand
            {
                WagonId = request.WagonId,
                TargetSlotId = request.TargetSlotId,
            };
            var result = await handler.Handle(command, ct);
            return result.IsSuccess
                ? Results.Ok(ApiResponse.Ok("Wagon moved."))
                : Results.BadRequest(ApiResponse.Fail(result.Error!));
        })
        .WithName("MoveWagon")
        .WithTags("Manevra")
        .RequirePermission(Permissions.Manevra.Edit);
    }
}
