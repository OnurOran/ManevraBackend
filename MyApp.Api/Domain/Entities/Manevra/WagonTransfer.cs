namespace MyApp.Api.Domain.Entities.Manevra;

public class WagonTransfer
{
    public int Id { get; private set; }
    public int WagonId { get; private set; }
    public int FromSlotId { get; private set; }
    public int ToSlotId { get; private set; }
    public bool IsApproved { get; private set; }
    public DateTime RequestedAt { get; private set; }

    public Wagon Wagon { get; private set; } = null!;
    public TrackSlot FromSlot { get; private set; } = null!;
    public TrackSlot ToSlot { get; private set; } = null!;

    private WagonTransfer() { }

    public static WagonTransfer Create(int wagonId, int fromSlotId, int toSlotId)
    {
        return new WagonTransfer
        {
            WagonId = wagonId,
            FromSlotId = fromSlotId,
            ToSlotId = toSlotId,
            IsApproved = false,
            RequestedAt = DateTime.UtcNow,
        };
    }

    public void Approve() => IsApproved = true;
}
