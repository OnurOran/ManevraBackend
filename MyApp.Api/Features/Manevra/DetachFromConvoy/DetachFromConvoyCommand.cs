using MyApp.Api.Common.Behaviors;

namespace MyApp.Api.Features.Manevra.DetachFromConvoy;

public class DetachFromConvoyCommand : ICommand<bool>
{
    public int WagonId { get; set; }
}
