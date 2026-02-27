using MyApp.Api.Common.Behaviors;

namespace MyApp.Api.Features.Wagons.ToggleWagonMiddle;

public class ToggleWagonMiddleCommand : ICommand<bool>
{
    public int WagonId { get; set; }
}
