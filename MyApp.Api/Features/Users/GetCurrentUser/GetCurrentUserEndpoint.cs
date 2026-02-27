using MyApp.Api.Common.Behaviors;
using MyApp.Api.Common.Models;
using MyApp.Api.Common.Services;
using MyApp.Api.Contracts.Users;
using MyApp.Api.Common.Attributes;

namespace MyApp.Api.Features.Users.GetCurrentUser;

[MapToGroup("users")]
public static class GetCurrentUserEndpoint
{
    public static void Map(RouteGroupBuilder group)
    {
        group.MapGet("/me", async Task<IResult> (
            IQueryHandler<GetCurrentUserQuery, UserResponse> handler,
            ICurrentUserService currentUser,
            CancellationToken ct) =>
        {
            if (!currentUser.UserId.HasValue)
                return Results.Json(ApiResponse<UserResponse>.Fail("User identity could not be resolved."),
                    statusCode: StatusCodes.Status401Unauthorized);

            var query = new GetCurrentUserQuery { UserId = currentUser.UserId.Value };
            var result = await handler.Handle(query, ct);
            return result.IsSuccess
                ? Results.Ok(ApiResponse<UserResponse>.Ok(result.Value!))
                : Results.NotFound(ApiResponse<UserResponse>.Fail(result.Error!));
        })
        .WithName("GetCurrentUser")
        .WithTags("Users")
        .RequireAuthorization();
    }
}
