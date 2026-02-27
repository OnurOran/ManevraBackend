namespace MyApp.Api.Domain.Entities.Manevra;

public class TrackSlot
{
    public int Id { get; private set; }
    public int TrackId { get; private set; }
    public SectionType SectionType { get; private set; }
    public byte SlotIndex { get; private set; }
    public int? WagonId { get; private set; }

    public Track Track { get; private set; } = null!;
    public Wagon? Wagon { get; private set; }

    private TrackSlot() { }

    public static TrackSlot Create(int trackId, SectionType sectionType, byte slotIndex)
    {
        return new TrackSlot
        {
            TrackId = trackId,
            SectionType = sectionType,
            SlotIndex = slotIndex,
        };
    }

    public void SetWagonId(int? wagonId) => WagonId = wagonId;
}
