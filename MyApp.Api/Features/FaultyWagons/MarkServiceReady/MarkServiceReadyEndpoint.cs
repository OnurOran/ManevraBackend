using MyApp.Api.Common.Authorization;
using MyApp.Api.Common.Behaviors;
using MyApp.Api.Common.Extensions;
using MyApp.Api.Common.Models;
using MyApp.Api.Common.Attributes;

namespace MyApp.Api.Features.FaultyWagons.MarkServiceReady;

[MapToGroup("faulty-wagons")]
public static class MarkServiceReadyEndpoint
{
    public static void Map(RouteGroupBuilder group)
    {
        group.MapPost("/{id:int}/service-ready", async Task<IResult> (
            int id,
            ICommandHandler<MarkServiceReadyCommand, bool> handler,
            CancellationToken ct) =>
        {
            var command = new MarkServiceReadyCommand { FaultyEntryId = id };
            var result = await handler.Handle(command, ct);
            return result.IsSuccess
                ? Results.Ok(ApiResponse.Ok())
                : Results.BadRequest(ApiResponse.Fail(result.Error!));
        })
        .WithName("MarkServiceReady")
        .WithTags("FaultyWagons")
        .RequirePermission(Permissions.Field.View);
    }
}
