using MyApp.Api.Common.Authorization;
using MyApp.Api.Common.Behaviors;
using MyApp.Api.Common.Extensions;
using MyApp.Api.Common.Models;
using MyApp.Api.Contracts.Manevra;
using MyApp.Api.Common.Attributes;

namespace MyApp.Api.Features.WeeklyMaintenance.UpsertMaintenanceEntry;

[MapToGroup("weekly-maintenance")]
public static class UpsertMaintenanceEntryEndpoint
{
    public static void Map(RouteGroupBuilder group)
    {
        group.MapPost("/", async Task<IResult> (
            UpsertMaintenanceEntryRequest request,
            ICommandHandler<UpsertMaintenanceEntryCommand, WeeklyMaintenanceEntryResponse> handler,
            CancellationToken ct) =>
        {
            if (!DateOnly.TryParse(request.WeekStartDate, out var weekStart))
                return Results.BadRequest(ApiResponse<WeeklyMaintenanceEntryResponse>.Fail("Invalid date format."));

            var command = new UpsertMaintenanceEntryCommand
            {
                WagonId = request.WagonId,
                TableType = request.TableType,
                WeekStartDate = weekStart,
                DayOfWeek = request.DayOfWeek,
                ShiftType = request.ShiftType,
                SlotIndex = request.SlotIndex,
                Priority = request.Priority,
            };
            var result = await handler.Handle(command, ct);
            return result.IsSuccess
                ? Results.Ok(ApiResponse<WeeklyMaintenanceEntryResponse>.Ok(result.Value!))
                : Results.BadRequest(ApiResponse<WeeklyMaintenanceEntryResponse>.Fail(result.Error!));
        })
        .WithName("UpsertMaintenanceEntry")
        .WithTags("WeeklyMaintenance")
        .RequirePermission(Permissions.WeeklyMaintenance.Edit);
    }
}
