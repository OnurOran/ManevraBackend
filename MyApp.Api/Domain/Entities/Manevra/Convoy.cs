namespace MyApp.Api.Domain.Entities.Manevra;

public class Convoy
{
    public Guid Id { get; private set; }
    public DateTime CreatedAt { get; private set; }

    public ICollection<Wagon> Wagons { get; private set; } = [];

    private Convoy() { }

    public static Convoy Create()
    {
        return new Convoy
        {
            Id = Guid.NewGuid(),
            CreatedAt = DateTime.UtcNow,
        };
    }
}
