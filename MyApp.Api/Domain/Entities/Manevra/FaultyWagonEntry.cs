namespace MyApp.Api.Domain.Entities.Manevra;

public class FaultyWagonEntry
{
    public int Id { get; private set; }
    public int WagonId { get; private set; }
    public DateTime CreatedAt { get; private set; }

    public Wagon Wagon { get; private set; } = null!;

    private FaultyWagonEntry() { }

    public static FaultyWagonEntry Create(int wagonId)
    {
        return new FaultyWagonEntry
        {
            WagonId = wagonId,
            CreatedAt = DateTime.UtcNow,
        };
    }
}
