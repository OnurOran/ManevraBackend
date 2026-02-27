namespace MyApp.Api.Domain.Entities.Manevra;

public class CleanupHistory
{
    public int Id { get; private set; }
    public int WagonId { get; private set; }
    public DateTime CleanupDate { get; private set; }

    public Wagon Wagon { get; private set; } = null!;

    private CleanupHistory() { }

    public static CleanupHistory Create(int wagonId, DateTime cleanupDate)
    {
        return new CleanupHistory
        {
            WagonId = wagonId,
            CleanupDate = cleanupDate,
        };
    }
}
