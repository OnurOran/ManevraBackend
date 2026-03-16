using MyApp.Api.Common.Behaviors;

namespace MyApp.Api.Features.WeeklyMaintenance.UpdateMaintenancePriority;

public class UpdateMaintenancePriorityCommand : ICommand<bool>
{
    public int EntryId { get; set; }
    public byte? Priority { get; set; }
}
