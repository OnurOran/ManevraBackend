using MyApp.Api.Common.Behaviors;
using MyApp.Api.Contracts.Manevra;

namespace MyApp.Api.Features.Cleanup.AddCleanup;

public class AddCleanupCommand : ICommand<CleanupEntryResponse>
{
    public int WagonId { get; set; }
    public DateTime CleanupDate { get; set; }
}
