using MyApp.Api.Common.Authorization;
using MyApp.Api.Common.Behaviors;
using MyApp.Api.Common.Extensions;
using MyApp.Api.Common.Models;
using MyApp.Api.Common.Attributes;

namespace MyApp.Api.Features.Manevra.DisbandConvoy;

[MapToGroup("manevra")]
public static class DisbandConvoyEndpoint
{
    public static void Map(RouteGroupBuilder group)
    {
        group.MapDelete("/convoy/{id:guid}", async Task<IResult> (
            Guid id,
            ICommandHandler<DisbandConvoyCommand, bool> handler,
            CancellationToken ct) =>
        {
            var command = new DisbandConvoyCommand { ConvoyId = id };
            var result = await handler.Handle(command, ct);
            return result.IsSuccess
                ? Results.Ok(ApiResponse.Ok("Convoy disbanded."))
                : Results.BadRequest(ApiResponse.Fail(result.Error!));
        })
        .WithName("DisbandConvoy")
        .WithTags("Manevra")
        .RequirePermission(Permissions.Manevra.Edit);
    }
}
