using MyApp.Api.Common.Behaviors;

namespace MyApp.Api.Features.Wagons.UpdateWagonStatus;

public class UpdateWagonStatusCommand : ICommand<bool>
{
    public int WagonId { get; set; }
    public byte Status { get; set; }
}
