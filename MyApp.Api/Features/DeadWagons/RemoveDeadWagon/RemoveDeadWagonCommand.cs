using MyApp.Api.Common.Behaviors;

namespace MyApp.Api.Features.DeadWagons.RemoveDeadWagon;

public class RemoveDeadWagonCommand : ICommand<bool>
{
    public int EntryId { get; set; }
}
