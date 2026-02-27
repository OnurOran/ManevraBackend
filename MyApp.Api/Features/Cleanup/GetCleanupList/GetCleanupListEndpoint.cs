using MyApp.Api.Common.Authorization;
using MyApp.Api.Common.Behaviors;
using MyApp.Api.Common.Extensions;
using MyApp.Api.Common.Models;
using MyApp.Api.Contracts.Manevra;
using MyApp.Api.Common.Attributes;

namespace MyApp.Api.Features.Cleanup.GetCleanupList;

[MapToGroup("cleanup")]
public static class GetCleanupListEndpoint
{
    public static void Map(RouteGroupBuilder group)
    {
        group.MapGet("/", async Task<IResult> (
            IQueryHandler<GetCleanupListQuery, List<CleanupEntryResponse>> handler,
            CancellationToken ct) =>
        {
            var result = await handler.Handle(new GetCleanupListQuery(), ct);
            return result.IsSuccess
                ? Results.Ok(ApiResponse<List<CleanupEntryResponse>>.Ok(result.Value!))
                : Results.BadRequest(ApiResponse<List<CleanupEntryResponse>>.Fail(result.Error!));
        })
        .WithName("GetCleanupList")
        .WithTags("Cleanup")
        .RequirePermission(Permissions.Cleanup.View);
    }
}
