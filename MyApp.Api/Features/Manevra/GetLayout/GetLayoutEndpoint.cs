using MyApp.Api.Common.Authorization;
using MyApp.Api.Common.Behaviors;
using MyApp.Api.Common.Extensions;
using MyApp.Api.Common.Models;
using MyApp.Api.Contracts.Manevra;
using MyApp.Api.Common.Attributes;

namespace MyApp.Api.Features.Manevra.GetLayout;

[MapToGroup("manevra")]
public static class GetLayoutEndpoint
{
    public static void Map(RouteGroupBuilder group)
    {
        group.MapGet("/layout", async Task<IResult> (
            IQueryHandler<GetLayoutQuery, LayoutResponse> handler,
            CancellationToken ct) =>
        {
            var result = await handler.Handle(new GetLayoutQuery(), ct);
            return result.IsSuccess
                ? Results.Ok(ApiResponse<LayoutResponse>.Ok(result.Value!))
                : Results.BadRequest(ApiResponse<LayoutResponse>.Fail(result.Error!));
        })
        .WithName("GetLayout")
        .WithTags("Manevra")
        .RequirePermission(Permissions.Manevra.View);
    }
}
