namespace MyApp.Api.Contracts.Manevra;

public class CreateWagonRequest
{
    public int WagonNumber { get; set; }
    public byte Line { get; set; }
    public string TechnicalUnit { get; set; } = string.Empty;
}

public class UpdateWagonStatusRequest
{
    public byte Status { get; set; }
}

public class WagonResponse
{
    public int Id { get; set; }
    public int WagonNumber { get; set; }
    public byte Line { get; set; }
    public string TechnicalUnit { get; set; } = string.Empty;
    public bool IsOnlyMiddle { get; set; }
    public byte Status { get; set; }
    public Guid? ConvoyId { get; set; }
    public int? SlotId { get; set; }
}
