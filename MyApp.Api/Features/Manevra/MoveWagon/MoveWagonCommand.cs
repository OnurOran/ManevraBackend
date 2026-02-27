using MyApp.Api.Common.Behaviors;

namespace MyApp.Api.Features.Manevra.MoveWagon;

public class MoveWagonCommand : ICommand<bool>
{
    public int WagonId { get; set; }
    public int TargetSlotId { get; set; }
}
