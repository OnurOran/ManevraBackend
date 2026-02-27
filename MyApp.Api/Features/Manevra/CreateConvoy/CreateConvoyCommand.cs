using MyApp.Api.Common.Behaviors;

namespace MyApp.Api.Features.Manevra.CreateConvoy;

public class CreateConvoyCommand : ICommand<Guid>
{
    public List<int> WagonIds { get; set; } = [];
}
