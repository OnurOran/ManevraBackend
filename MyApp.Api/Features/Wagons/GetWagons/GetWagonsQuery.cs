using MyApp.Api.Common.Behaviors;
using MyApp.Api.Contracts.Manevra;

namespace MyApp.Api.Features.Wagons.GetWagons;

public class GetWagonsQuery : IQuery<List<WagonResponse>>
{
    public byte? Line { get; set; }
}
