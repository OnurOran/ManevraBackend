using MyApp.Api.Common.Behaviors;

namespace MyApp.Api.Features.Manevra.DisbandConvoy;

public class DisbandConvoyCommand : ICommand<bool>
{
    public Guid ConvoyId { get; set; }
}
