using MyApp.Api.Common.Authorization;
using MyApp.Api.Common.Behaviors;
using MyApp.Api.Common.Extensions;
using MyApp.Api.Common.Models;
using MyApp.Api.Contracts.Manevra;
using MyApp.Api.Common.Attributes;

namespace MyApp.Api.Features.WeeklyMaintenance.GetWeeklyMaintenance;

[MapToGroup("weekly-maintenance")]
public static class GetWeeklyMaintenanceEndpoint
{
    public static void Map(RouteGroupBuilder group)
    {
        group.MapGet("/", async Task<IResult> (
            string weekStartDate,
            IQueryHandler<GetWeeklyMaintenanceQuery, List<WeeklyMaintenanceEntryResponse>> handler,
            CancellationToken ct) =>
        {
            if (!DateOnly.TryParse(weekStartDate, out var parsed))
                return Results.BadRequest(ApiResponse<List<WeeklyMaintenanceEntryResponse>>.Fail("Invalid date format."));

            var query = new GetWeeklyMaintenanceQuery { WeekStartDate = parsed };
            var result = await handler.Handle(query, ct);
            return result.IsSuccess
                ? Results.Ok(ApiResponse<List<WeeklyMaintenanceEntryResponse>>.Ok(result.Value!))
                : Results.BadRequest(ApiResponse<List<WeeklyMaintenanceEntryResponse>>.Fail(result.Error!));
        })
        .WithName("GetWeeklyMaintenance")
        .WithTags("WeeklyMaintenance")
        .RequirePermission(Permissions.WeeklyMaintenance.View);
    }
}
