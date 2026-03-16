using MyApp.Api.Common.Behaviors;
using MyApp.Api.Contracts.Manevra;

namespace MyApp.Api.Features.WeeklyMaintenance.GetWeeklyMaintenance;

public class GetWeeklyMaintenanceQuery : IQuery<List<WeeklyMaintenanceEntryResponse>>
{
    public DateOnly WeekStartDate { get; set; }
}
