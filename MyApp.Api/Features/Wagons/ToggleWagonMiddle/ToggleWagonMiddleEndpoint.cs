using MyApp.Api.Common.Authorization;
using MyApp.Api.Common.Behaviors;
using MyApp.Api.Common.Extensions;
using MyApp.Api.Common.Models;
using MyApp.Api.Common.Attributes;

namespace MyApp.Api.Features.Wagons.ToggleWagonMiddle;

[MapToGroup("wagons")]
public static class ToggleWagonMiddleEndpoint
{
    public static void Map(RouteGroupBuilder group)
    {
        group.MapPut("/{id:int}/toggle-middle", async Task<IResult> (
            int id,
            ICommandHandler<ToggleWagonMiddleCommand, bool> handler,
            CancellationToken ct) =>
        {
            var command = new ToggleWagonMiddleCommand { WagonId = id };
            var result = await handler.Handle(command, ct);
            return result.IsSuccess
                ? Results.Ok(ApiResponse.Ok("IsOnlyMiddle toggled."))
                : Results.BadRequest(ApiResponse.Fail(result.Error!));
        })
        .WithName("ToggleWagonMiddle")
        .WithTags("Wagons")
        .RequirePermission(Permissions.Wagons.Edit);
    }
}
