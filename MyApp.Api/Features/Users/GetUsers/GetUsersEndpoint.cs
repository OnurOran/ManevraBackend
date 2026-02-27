using MyApp.Api.Common.Authorization;
using MyApp.Api.Common.Behaviors;
using MyApp.Api.Common.Extensions;
using MyApp.Api.Common.Models;
using MyApp.Api.Contracts.Users;
using MyApp.Api.Common.Attributes;

namespace MyApp.Api.Features.Users.GetUsers;

[MapToGroup("users")]
public static class GetUsersEndpoint
{
    public static void Map(RouteGroupBuilder group)
    {
        group.MapGet("/", async Task<IResult> (
            IQueryHandler<GetUsersQuery, PagedResult<UserListItemResponse>> handler,
            int page = 1,
            int pageSize = 10,
            string? search = null,
            string? sortBy = null,
            bool sortDesc = false,
            CancellationToken ct = default) =>
        {
            var query = new GetUsersQuery
            {
                Page     = page,
                PageSize = pageSize,
                Search   = search,
                SortBy   = sortBy,
                SortDesc = sortDesc,
            };

            var result = await handler.Handle(query, ct);
            return result.IsSuccess
                ? Results.Ok(ApiResponse<PagedResult<UserListItemResponse>>.Ok(result.Value!))
                : Results.BadRequest(ApiResponse<PagedResult<UserListItemResponse>>.Fail(result.Error!));
        })
        .WithName("GetUsers")
        .WithTags("Users")
        .RequirePermission(Permissions.Users.View);
    }
}
