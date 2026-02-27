using MyApp.Api.Common.Authorization;
using MyApp.Api.Common.Behaviors;
using MyApp.Api.Common.Extensions;
using MyApp.Api.Common.Models;
using MyApp.Api.Common.Attributes;

namespace MyApp.Api.Features.Manevra.ApproveTransfer;

[MapToGroup("manevra")]
public static class ApproveTransferEndpoint
{
    public static void Map(RouteGroupBuilder group)
    {
        group.MapPost("/transfers/{id:int}/approve", async Task<IResult> (
            int id,
            ICommandHandler<ApproveTransferCommand, bool> handler,
            CancellationToken ct) =>
        {
            var command = new ApproveTransferCommand { TransferId = id };
            var result = await handler.Handle(command, ct);
            return result.IsSuccess
                ? Results.Ok(ApiResponse.Ok("Transfer approved."))
                : Results.BadRequest(ApiResponse.Fail(result.Error!));
        })
        .WithName("ApproveTransfer")
        .WithTags("Manevra")
        .RequirePermission(Permissions.Manevra.Approve);
    }
}
