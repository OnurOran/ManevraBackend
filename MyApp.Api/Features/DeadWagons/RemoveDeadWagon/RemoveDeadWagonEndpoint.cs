using MyApp.Api.Common.Authorization;
using MyApp.Api.Common.Behaviors;
using MyApp.Api.Common.Extensions;
using MyApp.Api.Common.Models;
using MyApp.Api.Common.Attributes;

namespace MyApp.Api.Features.DeadWagons.RemoveDeadWagon;

[MapToGroup("dead-wagons")]
public static class RemoveDeadWagonEndpoint
{
    public static void Map(RouteGroupBuilder group)
    {
        group.MapDelete("/{id:int}", async Task<IResult> (
            int id,
            ICommandHandler<RemoveDeadWagonCommand, bool> handler,
            CancellationToken ct) =>
        {
            var command = new RemoveDeadWagonCommand { EntryId = id };
            var result = await handler.Handle(command, ct);
            return result.IsSuccess
                ? Results.Ok(ApiResponse.Ok())
                : Results.BadRequest(ApiResponse.Fail(result.Error!));
        })
        .WithName("RemoveDeadWagon")
        .WithTags("DeadWagons")
        .RequirePermission(Permissions.Manevra.Edit);
    }
}
