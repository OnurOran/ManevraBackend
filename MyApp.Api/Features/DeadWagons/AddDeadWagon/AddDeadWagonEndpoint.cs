using MyApp.Api.Common.Authorization;
using MyApp.Api.Common.Behaviors;
using MyApp.Api.Common.Extensions;
using MyApp.Api.Common.Models;
using MyApp.Api.Contracts.Manevra;
using MyApp.Api.Common.Attributes;

namespace MyApp.Api.Features.DeadWagons.AddDeadWagon;

[MapToGroup("dead-wagons")]
public static class AddDeadWagonEndpoint
{
    public static void Map(RouteGroupBuilder group)
    {
        group.MapPost("/", async Task<IResult> (
            AddDeadWagonRequest request,
            ICommandHandler<AddDeadWagonCommand, DeadWagonResponse> handler,
            CancellationToken ct) =>
        {
            var command = new AddDeadWagonCommand { WagonId = request.WagonId };
            var result = await handler.Handle(command, ct);
            return result.IsSuccess
                ? Results.Ok(ApiResponse<DeadWagonResponse>.Ok(result.Value!))
                : Results.BadRequest(ApiResponse<DeadWagonResponse>.Fail(result.Error!));
        })
        .WithName("AddDeadWagon")
        .WithTags("DeadWagons")
        .RequirePermission(Permissions.Manevra.Edit);
    }
}
