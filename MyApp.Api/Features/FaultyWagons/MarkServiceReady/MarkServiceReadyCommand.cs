using MyApp.Api.Common.Behaviors;

namespace MyApp.Api.Features.FaultyWagons.MarkServiceReady;

public class MarkServiceReadyCommand : ICommand<bool>
{
    public int FaultyEntryId { get; set; }
}
