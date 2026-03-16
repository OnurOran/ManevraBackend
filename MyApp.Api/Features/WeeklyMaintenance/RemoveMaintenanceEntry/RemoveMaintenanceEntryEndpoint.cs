using MyApp.Api.Common.Authorization;
using MyApp.Api.Common.Behaviors;
using MyApp.Api.Common.Extensions;
using MyApp.Api.Common.Models;
using MyApp.Api.Common.Attributes;

namespace MyApp.Api.Features.WeeklyMaintenance.RemoveMaintenanceEntry;

[MapToGroup("weekly-maintenance")]
public static class RemoveMaintenanceEntryEndpoint
{
    public static void Map(RouteGroupBuilder group)
    {
        group.MapDelete("/{id:int}", async Task<IResult> (
            int id,
            ICommandHandler<RemoveMaintenanceEntryCommand, bool> handler,
            CancellationToken ct) =>
        {
            var command = new RemoveMaintenanceEntryCommand { EntryId = id };
            var result = await handler.Handle(command, ct);
            return result.IsSuccess
                ? Results.Ok(ApiResponse.Ok())
                : Results.BadRequest(ApiResponse.Fail(result.Error!));
        })
        .WithName("RemoveMaintenanceEntry")
        .WithTags("WeeklyMaintenance")
        .RequirePermission(Permissions.WeeklyMaintenance.Edit);
    }
}
