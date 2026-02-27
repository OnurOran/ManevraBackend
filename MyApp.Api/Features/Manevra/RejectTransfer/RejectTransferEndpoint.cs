using MyApp.Api.Common.Authorization;
using MyApp.Api.Common.Behaviors;
using MyApp.Api.Common.Extensions;
using MyApp.Api.Common.Models;
using MyApp.Api.Common.Attributes;

namespace MyApp.Api.Features.Manevra.RejectTransfer;

[MapToGroup("manevra")]
public static class RejectTransferEndpoint
{
    public static void Map(RouteGroupBuilder group)
    {
        group.MapDelete("/transfers/{id:int}", async Task<IResult> (
            int id,
            ICommandHandler<RejectTransferCommand, bool> handler,
            CancellationToken ct) =>
        {
            var command = new RejectTransferCommand { TransferId = id };
            var result = await handler.Handle(command, ct);
            return result.IsSuccess
                ? Results.Ok(ApiResponse.Ok("Transfer rejected."))
                : Results.BadRequest(ApiResponse.Fail(result.Error!));
        })
        .WithName("RejectTransfer")
        .WithTags("Manevra")
        .RequirePermission(Permissions.Manevra.Approve);
    }
}
