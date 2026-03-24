namespace MyApp.Api.Domain.Entities.Manevra;

public class DeadWagonEntry
{
    public int Id { get; private set; }
    public int WagonId { get; private set; }

    public Wagon Wagon { get; private set; } = null!;

    private DeadWagonEntry() { }

    public static DeadWagonEntry Create(int wagonId)
    {
        return new DeadWagonEntry { WagonId = wagonId };
    }
}
