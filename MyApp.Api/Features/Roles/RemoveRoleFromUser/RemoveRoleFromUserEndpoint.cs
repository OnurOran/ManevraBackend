using FluentValidation;
using MyApp.Api.Common.Extensions;
using MyApp.Api.Common.Authorization;
using MyApp.Api.Common.Behaviors;
using MyApp.Api.Common.Models;
using MyApp.Api.Common.Attributes;

namespace MyApp.Api.Features.Roles.RemoveRoleFromUser;

[MapToGroup("users")]
public static class RemoveRoleFromUserEndpoint
{
    public static void Map(RouteGroupBuilder group)
    {
        group.MapDelete("/{userId:guid}/roles/{roleName}", async Task<IResult> (
            Guid userId,
            string roleName,
            ICommandHandler<RemoveRoleFromUserCommand, bool> handler,
            IValidator<RemoveRoleFromUserCommand> validator,
            CancellationToken ct) =>
        {
            var command = new RemoveRoleFromUserCommand { UserId = userId, RoleName = roleName };

            var (isValid, errorResult) = await validator.ValidateRequestAsync(command, ct);
            if (!isValid) return errorResult!;

            var result = await handler.Handle(command, ct);
            return result.IsSuccess
                ? Results.NoContent()
                : Results.BadRequest(ApiResponse.Fail(result.Error!));
        })
        .WithName("RemoveRoleFromUser")
        .WithTags("Roles")
        .RequirePermission(Permissions.Roles.Manage);
    }
}
