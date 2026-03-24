using MyApp.Api.Common.Authorization;
using MyApp.Api.Common.Behaviors;
using MyApp.Api.Common.Extensions;
using MyApp.Api.Common.Models;
using MyApp.Api.Common.Attributes;

namespace MyApp.Api.Features.Wagons.UnplaceWagon;

[MapToGroup("wagons")]
public static class UnplaceWagonEndpoint
{
    public static void Map(RouteGroupBuilder group)
    {
        group.MapPost("/{id:int}/unplace", async Task<IResult> (
            int id,
            ICommandHandler<UnplaceWagonCommand, bool> handler,
            CancellationToken ct) =>
        {
            var command = new UnplaceWagonCommand { WagonId = id };
            var result = await handler.Handle(command, ct);
            return result.IsSuccess
                ? Results.Ok(ApiResponse.Ok())
                : Results.BadRequest(ApiResponse.Fail(result.Error!));
        })
        .WithName("UnplaceWagon")
        .WithTags("Wagons")
        .RequirePermission(Permissions.Wagons.Edit);
    }
}
