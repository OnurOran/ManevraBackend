namespace MyApp.Api.Contracts.Manevra;

public class AddCleanupRequest
{
    public int WagonId { get; set; }
    public DateTime CleanupDate { get; set; }
}

public class CleanupEntryResponse
{
    public int WagonId { get; set; }
    public int WagonNumber { get; set; }
    public byte Line { get; set; }
    public DateTime CleanupDate { get; set; }
    public DateTime? PreviousCleanupDate { get; set; }
}
