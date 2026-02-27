using MyApp.Api.Common.Extensions;
using MyApp.Api.Common.Authorization;
using MyApp.Api.Common.Behaviors;
using MyApp.Api.Common.Models;
using MyApp.Api.Contracts.Roles;
using MyApp.Api.Common.Attributes;

namespace MyApp.Api.Features.Roles.GetRoles;

[MapToGroup("roles")]
public static class GetRolesEndpoint
{
    public static void Map(RouteGroupBuilder group)
    {
        group.MapGet("/", async Task<IResult> (
            IQueryHandler<GetRolesQuery, IEnumerable<RoleResponse>> handler,
            CancellationToken ct) =>
        {
            var result = await handler.Handle(new GetRolesQuery(), ct);
            return result.IsSuccess
                ? Results.Ok(ApiResponse<IEnumerable<RoleResponse>>.Ok(result.Value!))
                : Results.BadRequest(ApiResponse<IEnumerable<RoleResponse>>.Fail(result.Error!));
        })
        .WithName("GetRoles")
        .WithTags("Roles")
        .RequirePermission(Permissions.Roles.View);
    }
}
