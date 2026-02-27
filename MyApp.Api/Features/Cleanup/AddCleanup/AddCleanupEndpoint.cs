using MyApp.Api.Common.Authorization;
using MyApp.Api.Common.Behaviors;
using MyApp.Api.Common.Extensions;
using MyApp.Api.Common.Models;
using MyApp.Api.Contracts.Manevra;
using MyApp.Api.Common.Attributes;

namespace MyApp.Api.Features.Cleanup.AddCleanup;

[MapToGroup("cleanup")]
public static class AddCleanupEndpoint
{
    public static void Map(RouteGroupBuilder group)
    {
        group.MapPost("/", async Task<IResult> (
            AddCleanupRequest request,
            ICommandHandler<AddCleanupCommand, CleanupEntryResponse> handler,
            CancellationToken ct) =>
        {
            var command = new AddCleanupCommand
            {
                WagonId = request.WagonId,
                CleanupDate = request.CleanupDate,
            };
            var result = await handler.Handle(command, ct);
            return result.IsSuccess
                ? Results.Created("/api/v1/cleanup", ApiResponse<CleanupEntryResponse>.Ok(result.Value!))
                : Results.BadRequest(ApiResponse<CleanupEntryResponse>.Fail(result.Error!));
        })
        .WithName("AddCleanup")
        .WithTags("Cleanup")
        .RequirePermission(Permissions.Cleanup.Edit);
    }
}
