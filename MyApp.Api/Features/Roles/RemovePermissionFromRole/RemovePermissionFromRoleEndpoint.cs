using FluentValidation;
using MyApp.Api.Common.Extensions;
using MyApp.Api.Common.Authorization;
using MyApp.Api.Common.Behaviors;
using MyApp.Api.Common.Models;
using MyApp.Api.Common.Attributes;

namespace MyApp.Api.Features.Roles.RemovePermissionFromRole;

[MapToGroup("roles")]
public static class RemovePermissionFromRoleEndpoint
{
    public static void Map(RouteGroupBuilder group)
    {
        group.MapDelete("/{roleId:guid}/permissions/{permissionId:guid}", async Task<IResult> (
            Guid roleId,
            Guid permissionId,
            ICommandHandler<RemovePermissionFromRoleCommand, bool> handler,
            IValidator<RemovePermissionFromRoleCommand> validator,
            CancellationToken ct) =>
        {
            var command = new RemovePermissionFromRoleCommand { RoleId = roleId, PermissionId = permissionId };

            var (isValid, errorResult) = await validator.ValidateRequestAsync(command, ct);
            if (!isValid) return errorResult!;

            var result = await handler.Handle(command, ct);
            return result.IsSuccess
                ? Results.NoContent()
                : Results.BadRequest(ApiResponse.Fail(result.Error!));
        })
        .WithName("RemovePermissionFromRole")
        .WithTags("Roles")
        .RequirePermission(Permissions.Roles.Manage);
    }
}
