using MyApp.Api.Common.Behaviors;
using MyApp.Api.Contracts.Manevra;

namespace MyApp.Api.Features.Wagons.CreateWagon;

public class CreateWagonCommand : ICommand<WagonResponse>
{
    public int WagonNumber { get; set; }
    public byte Line { get; set; }
    public string TechnicalUnit { get; set; } = string.Empty;
}
