using MyApp.Api.Common.Behaviors;

namespace MyApp.Api.Features.Wagons.UnplaceWagon;

public class UnplaceWagonCommand : ICommand<bool>
{
    public int WagonId { get; set; }
}
