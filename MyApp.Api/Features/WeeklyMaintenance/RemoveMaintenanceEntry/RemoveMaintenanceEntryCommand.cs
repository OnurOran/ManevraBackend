using MyApp.Api.Common.Behaviors;

namespace MyApp.Api.Features.WeeklyMaintenance.RemoveMaintenanceEntry;

public class RemoveMaintenanceEntryCommand : ICommand<bool>
{
    public int EntryId { get; set; }
}
