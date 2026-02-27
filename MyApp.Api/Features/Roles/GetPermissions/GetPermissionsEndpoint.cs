using MyApp.Api.Common.Extensions;
using MyApp.Api.Common.Authorization;
using MyApp.Api.Common.Behaviors;
using MyApp.Api.Common.Models;
using MyApp.Api.Contracts.Roles;
using MyApp.Api.Common.Attributes;

namespace MyApp.Api.Features.Roles.GetPermissions;

[MapToGroup("roles")]
public static class GetPermissionsEndpoint
{
    public static void Map(RouteGroupBuilder group)
    {
        group.MapGet("/permissions", async Task<IResult> (
            IQueryHandler<GetPermissionsQuery, IEnumerable<PermissionResponse>> handler,
            CancellationToken ct) =>
        {
            var result = await handler.Handle(new GetPermissionsQuery(), ct);
            return result.IsSuccess
                ? Results.Ok(ApiResponse<IEnumerable<PermissionResponse>>.Ok(result.Value!))
                : Results.BadRequest(ApiResponse<IEnumerable<PermissionResponse>>.Fail(result.Error!));
        })
        .WithName("GetPermissions")
        .WithTags("Roles")
        .RequirePermission(Permissions.Roles.View);
    }
}
