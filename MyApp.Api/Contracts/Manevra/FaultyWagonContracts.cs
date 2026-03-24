namespace MyApp.Api.Contracts.Manevra;

public class FaultyWagonResponse
{
    public int Id { get; set; }
    public int WagonId { get; set; }
    public int WagonNumber { get; set; }
    public byte Line { get; set; }
    public byte Status { get; set; }
    public string Location { get; set; } = string.Empty;
    public byte SlotIndex { get; set; }
    public DateTime CreatedAt { get; set; }
}
