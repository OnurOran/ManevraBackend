using MyApp.Api.Common.Authorization;
using MyApp.Api.Common.Behaviors;
using MyApp.Api.Common.Extensions;
using MyApp.Api.Common.Models;
using MyApp.Api.Contracts.Users;
using MyApp.Api.Common.Attributes;

namespace MyApp.Api.Features.Users.GetUserById;

[MapToGroup("users")]
public static class GetUserByIdEndpoint
{
    public static void Map(RouteGroupBuilder group)
    {
        group.MapGet("/{userId:guid}", async Task<IResult> (
            Guid userId,
            IQueryHandler<GetUserByIdQuery, UserListItemResponse> handler,
            CancellationToken ct) =>
        {
            var query = new GetUserByIdQuery { UserId = userId };
            var result = await handler.Handle(query, ct);
            return result.IsSuccess
                ? Results.Ok(ApiResponse<UserListItemResponse>.Ok(result.Value!))
                : Results.NotFound(ApiResponse<UserListItemResponse>.Fail(result.Error!));
        })
        .WithName("GetUserById")
        .WithTags("Users")
        .RequirePermission(Permissions.Users.View);
    }
}
