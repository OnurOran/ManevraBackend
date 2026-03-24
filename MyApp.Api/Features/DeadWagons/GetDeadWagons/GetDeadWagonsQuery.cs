using MyApp.Api.Common.Behaviors;
using MyApp.Api.Contracts.Manevra;

namespace MyApp.Api.Features.DeadWagons.GetDeadWagons;

public class GetDeadWagonsQuery : IQuery<List<DeadWagonResponse>> { }
