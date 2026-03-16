namespace MyApp.Api.Contracts.Manevra;

public class WeeklyMaintenanceEntryResponse
{
    public int Id { get; set; }
    public int WagonId { get; set; }
    public int WagonNumber { get; set; }
    public byte Line { get; set; }
    public byte Status { get; set; }
    public byte TableType { get; set; }
    public string WeekStartDate { get; set; } = null!;
    public byte DayOfWeek { get; set; }
    public byte ShiftType { get; set; }
    public byte SlotIndex { get; set; }
    public byte? Priority { get; set; }
}

public class UpsertMaintenanceEntryRequest
{
    public int WagonId { get; set; }
    public byte TableType { get; set; }
    public string WeekStartDate { get; set; } = null!;
    public byte DayOfWeek { get; set; }
    public byte ShiftType { get; set; }
    public byte SlotIndex { get; set; }
    public byte? Priority { get; set; }
}

public class UpdateMaintenancePriorityRequest
{
    public byte? Priority { get; set; }
}
