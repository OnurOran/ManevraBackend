using MyApp.Api.Common.Authorization;
using MyApp.Api.Common.Behaviors;
using MyApp.Api.Common.Extensions;
using MyApp.Api.Common.Models;
using MyApp.Api.Contracts.Manevra;
using MyApp.Api.Common.Attributes;

namespace MyApp.Api.Features.Wagons.UpdateWagonStatus;

[MapToGroup("wagons")]
public static class UpdateWagonStatusEndpoint
{
    public static void Map(RouteGroupBuilder group)
    {
        group.MapPut("/{id:int}/status", async Task<IResult> (
            int id,
            UpdateWagonStatusRequest request,
            ICommandHandler<UpdateWagonStatusCommand, bool> handler,
            CancellationToken ct) =>
        {
            var command = new UpdateWagonStatusCommand { WagonId = id, Status = request.Status };
            var result = await handler.Handle(command, ct);
            return result.IsSuccess
                ? Results.Ok(ApiResponse.Ok("Status updated."))
                : Results.BadRequest(ApiResponse.Fail(result.Error!));
        })
        .WithName("UpdateWagonStatus")
        .WithTags("Wagons")
        .RequirePermission(Permissions.Wagons.Edit);
    }
}
