namespace MyApp.Api.Contracts.Manevra;

public class MoveWagonRequest
{
    public int WagonId { get; set; }
    public int TargetSlotId { get; set; }
}

public class CreateConvoyRequest
{
    public List<int> WagonIds { get; set; } = [];
}

public class DetachFromConvoyRequest
{
    public int WagonId { get; set; }
}

public class TrackSlotResponse
{
    public int Id { get; set; }
    public int TrackId { get; set; }
    public byte SectionType { get; set; }
    public byte SlotIndex { get; set; }
    public WagonResponse? Wagon { get; set; }
    public PendingTransferResponse? PendingTransfer { get; set; }
}

public class TrackLayoutResponse
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public byte Zone { get; set; }
    public string? Note { get; set; }
    public List<TrackSlotResponse> Slots { get; set; } = [];
}

public class UpdateTrackNoteRequest
{
    public string? Note { get; set; }
}

public class PendingTransferResponse
{
    public int Id { get; set; }
    public int WagonId { get; set; }
    public int WagonNumber { get; set; }
    public int FromSlotId { get; set; }
    public int ToSlotId { get; set; }
    public DateTime RequestedAt { get; set; }
}

public class LayoutResponse
{
    public List<TrackLayoutResponse> Tracks { get; set; } = [];
    public List<PendingTransferResponse> PendingTransfers { get; set; } = [];
}
