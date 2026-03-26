namespace MyApp.Api.Contracts.Manevra;

public class DeadWagonResponse
{
    public int Id { get; set; }
    public int WagonId { get; set; }
    public int WagonNumber { get; set; }
    public byte Line { get; set; }
    public byte Status { get; set; }
    public bool IsOnlyMiddle { get; set; }
}

public class AddDeadWagonRequest
{
    public int WagonId { get; set; }
}
