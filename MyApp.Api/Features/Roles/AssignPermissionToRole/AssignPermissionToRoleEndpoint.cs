using FluentValidation;
using MyApp.Api.Common.Extensions;
using MyApp.Api.Common.Authorization;
using MyApp.Api.Common.Behaviors;
using MyApp.Api.Common.Models;
using MyApp.Api.Contracts.Roles;
using MyApp.Api.Common.Attributes;

namespace MyApp.Api.Features.Roles.AssignPermissionToRole;

[MapToGroup("roles")]
public static class AssignPermissionToRoleEndpoint
{
    public static void Map(RouteGroupBuilder group)
    {
        group.MapPost("/{roleId:guid}/permissions", async Task<IResult> (
            Guid roleId,
            AssignPermissionRequest request,
            ICommandHandler<AssignPermissionToRoleCommand, RoleResponse> handler,
            IValidator<AssignPermissionToRoleCommand> validator,
            CancellationToken ct) =>
        {
            var command = new AssignPermissionToRoleCommand
            {
                RoleId = roleId,
                PermissionId = request.PermissionId
            };

            var (isValid, errorResult) = await validator.ValidateRequestAsync(command, ct);
            if (!isValid) return errorResult!;

            var result = await handler.Handle(command, ct);
            return result.IsSuccess
                ? Results.Ok(ApiResponse<RoleResponse>.Ok(result.Value!))
                : Results.BadRequest(ApiResponse<RoleResponse>.Fail(result.Error!));
        })
        .WithName("AssignPermissionToRole")
        .WithTags("Roles")
        .RequirePermission(Permissions.Roles.Manage);
    }
}
