using MyApp.Api.Common.Behaviors;
using MyApp.Api.Contracts.Manevra;

namespace MyApp.Api.Features.DeadWagons.AddDeadWagon;

public class AddDeadWagonCommand : ICommand<DeadWagonResponse>
{
    public int WagonId { get; set; }
}
