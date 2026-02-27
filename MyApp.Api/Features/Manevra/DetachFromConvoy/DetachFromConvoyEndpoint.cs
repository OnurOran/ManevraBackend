using MyApp.Api.Common.Authorization;
using MyApp.Api.Common.Behaviors;
using MyApp.Api.Common.Extensions;
using MyApp.Api.Common.Models;
using MyApp.Api.Contracts.Manevra;
using MyApp.Api.Common.Attributes;

namespace MyApp.Api.Features.Manevra.DetachFromConvoy;

[MapToGroup("manevra")]
public static class DetachFromConvoyEndpoint
{
    public static void Map(RouteGroupBuilder group)
    {
        group.MapPost("/convoy/detach", async Task<IResult> (
            DetachFromConvoyRequest request,
            ICommandHandler<DetachFromConvoyCommand, bool> handler,
            CancellationToken ct) =>
        {
            var command = new DetachFromConvoyCommand { WagonId = request.WagonId };
            var result = await handler.Handle(command, ct);
            return result.IsSuccess
                ? Results.Ok(ApiResponse.Ok("Wagon detached from convoy."))
                : Results.BadRequest(ApiResponse.Fail(result.Error!));
        })
        .WithName("DetachFromConvoy")
        .WithTags("Manevra")
        .RequirePermission(Permissions.Manevra.Edit);
    }
}
