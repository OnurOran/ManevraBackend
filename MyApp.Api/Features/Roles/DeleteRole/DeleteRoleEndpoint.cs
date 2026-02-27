using FluentValidation;
using MyApp.Api.Common.Extensions;
using MyApp.Api.Common.Authorization;
using MyApp.Api.Common.Behaviors;
using MyApp.Api.Common.Models;
using MyApp.Api.Common.Attributes;

namespace MyApp.Api.Features.Roles.DeleteRole;

[MapToGroup("roles")]
public static class DeleteRoleEndpoint
{
    public static void Map(RouteGroupBuilder group)
    {
        group.MapDelete("/{roleId:guid}", async Task<IResult> (
            Guid roleId,
            ICommandHandler<DeleteRoleCommand, bool> handler,
            IValidator<DeleteRoleCommand> validator,
            CancellationToken ct) =>
        {
            var command = new DeleteRoleCommand { RoleId = roleId };

            var (isValid, errorResult) = await validator.ValidateRequestAsync(command, ct);
            if (!isValid) return errorResult!;

            var result = await handler.Handle(command, ct);
            return result.IsSuccess
                ? Results.NoContent()
                : Results.BadRequest(ApiResponse.Fail(result.Error!));
        })
        .WithName("DeleteRole")
        .WithTags("Roles")
        .RequirePermission(Permissions.Roles.Manage);
    }
}
