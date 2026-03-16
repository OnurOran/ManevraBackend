namespace MyApp.Api.Domain.Entities.Manevra;

public enum ShiftType : byte
{
    Gunduz = 1,
    Gece = 2,
}

public enum MaintenanceTableType : byte
{
    Bakim = 1,
    Torna = 2,
}

public class WeeklyMaintenanceEntry
{
    public int Id { get; private set; }
    public int WagonId { get; private set; }
    public MaintenanceTableType TableType { get; private set; }
    public DateOnly WeekStartDate { get; private set; }
    public byte DayOfWeek { get; private set; } // 0=Monday .. 5=Saturday
    public ShiftType ShiftType { get; private set; }
    public byte SlotIndex { get; private set; } // Bakim Gunduz: 1-2, Gece: 1-6 | Torna: 1-2
    public byte? Priority { get; private set; } // 1,2,3 (only for Bakim)

    public Wagon Wagon { get; private set; } = null!;

    private WeeklyMaintenanceEntry() { }

    public static WeeklyMaintenanceEntry Create(
        int wagonId,
        MaintenanceTableType tableType,
        DateOnly weekStartDate,
        byte dayOfWeek,
        ShiftType shiftType,
        byte slotIndex,
        byte? priority)
    {
        return new WeeklyMaintenanceEntry
        {
            WagonId = wagonId,
            TableType = tableType,
            WeekStartDate = weekStartDate,
            DayOfWeek = dayOfWeek,
            ShiftType = shiftType,
            SlotIndex = slotIndex,
            Priority = priority,
        };
    }

    public void SetPriority(byte? priority) => Priority = priority;
    public void SetWagonId(int wagonId) => WagonId = wagonId;
}
