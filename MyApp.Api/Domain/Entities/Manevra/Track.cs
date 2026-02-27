namespace MyApp.Api.Domain.Entities.Manevra;

public class Track
{
    public int Id { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public TrackZone Zone { get; private set; }

    public ICollection<TrackSlot> Slots { get; private set; } = [];

    private Track() { }

    public static Track Create(string name, TrackZone zone)
    {
        return new Track { Name = name, Zone = zone };
    }
}
