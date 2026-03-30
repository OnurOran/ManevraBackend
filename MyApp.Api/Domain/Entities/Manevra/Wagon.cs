namespace MyApp.Api.Domain.Entities.Manevra;

public class Wagon
{
    public int Id { get; private set; }
    public int WagonNumber { get; private set; }
    public WagonLine Line { get; private set; }
    public string TechnicalUnit { get; private set; } = string.Empty;
    public bool IsOnlyMiddle { get; private set; }
    public WagonStatus Status { get; private set; }
    public Guid? ConvoyId { get; private set; }

    public Convoy? Convoy { get; private set; }

    private Wagon() { }

    public static Wagon Create(int wagonNumber, WagonLine line, string technicalUnit)
    {
        var startsWithOne = wagonNumber.ToString()[0] == '1';
        return new Wagon
        {
            WagonNumber = wagonNumber,
            Line = line,
            TechnicalUnit = technicalUnit,
            IsOnlyMiddle = startsWithOne,
            Status = WagonStatus.Servis,
        };
    }

    public void SetStatus(WagonStatus status) => Status = status;
    public void SetConvoyId(Guid? convoyId) => ConvoyId = convoyId;
    public void ToggleIsOnlyMiddle() => IsOnlyMiddle = !IsOnlyMiddle;
}
