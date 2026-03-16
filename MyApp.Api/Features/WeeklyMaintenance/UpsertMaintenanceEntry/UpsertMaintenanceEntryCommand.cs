using MyApp.Api.Common.Behaviors;
using MyApp.Api.Contracts.Manevra;

namespace MyApp.Api.Features.WeeklyMaintenance.UpsertMaintenanceEntry;

public class UpsertMaintenanceEntryCommand : ICommand<WeeklyMaintenanceEntryResponse>
{
    public int WagonId { get; set; }
    public byte TableType { get; set; }
    public DateOnly WeekStartDate { get; set; }
    public byte DayOfWeek { get; set; }
    public byte ShiftType { get; set; }
    public byte SlotIndex { get; set; }
    public byte? Priority { get; set; }
}
