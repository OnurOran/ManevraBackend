using MyApp.Api.Common.Behaviors;
using MyApp.Api.Contracts.Manevra;

namespace MyApp.Api.Features.Cleanup.GetCleanupList;

public class GetCleanupListQuery : IQuery<List<CleanupEntryResponse>> { }
