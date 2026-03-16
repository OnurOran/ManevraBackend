using MyApp.Api.Common.Authorization;
using MyApp.Api.Common.Behaviors;
using MyApp.Api.Common.Extensions;
using MyApp.Api.Common.Models;
using MyApp.Api.Contracts.Manevra;
using MyApp.Api.Common.Attributes;

namespace MyApp.Api.Features.WeeklyMaintenance.UpdateMaintenancePriority;

[MapToGroup("weekly-maintenance")]
public static class UpdateMaintenancePriorityEndpoint
{
    public static void Map(RouteGroupBuilder group)
    {
        group.MapPut("/{id:int}/priority", async Task<IResult> (
            int id,
            UpdateMaintenancePriorityRequest request,
            ICommandHandler<UpdateMaintenancePriorityCommand, bool> handler,
            CancellationToken ct) =>
        {
            var command = new UpdateMaintenancePriorityCommand
            {
                EntryId = id,
                Priority = request.Priority,
            };
            var result = await handler.Handle(command, ct);
            return result.IsSuccess
                ? Results.Ok(ApiResponse.Ok())
                : Results.BadRequest(ApiResponse.Fail(result.Error!));
        })
        .WithName("UpdateMaintenancePriority")
        .WithTags("WeeklyMaintenance")
        .RequirePermission(Permissions.WeeklyMaintenance.Edit);
    }
}
