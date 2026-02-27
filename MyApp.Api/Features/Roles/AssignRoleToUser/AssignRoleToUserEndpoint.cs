using FluentValidation;
using MyApp.Api.Common.Extensions;
using MyApp.Api.Common.Authorization;
using MyApp.Api.Common.Behaviors;
using MyApp.Api.Common.Models;
using MyApp.Api.Contracts.Roles;
using MyApp.Api.Contracts.Users;
using MyApp.Api.Common.Attributes;

namespace MyApp.Api.Features.Roles.AssignRoleToUser;

[MapToGroup("users")]
public static class AssignRoleToUserEndpoint
{
    public static void Map(RouteGroupBuilder group)
    {
        group.MapPost("/{userId:guid}/roles", async Task<IResult> (
            Guid userId,
            AssignRoleRequest request,
            ICommandHandler<AssignRoleToUserCommand, UserResponse> handler,
            IValidator<AssignRoleToUserCommand> validator,
            CancellationToken ct) =>
        {
            var command = new AssignRoleToUserCommand { UserId = userId, RoleName = request.RoleName };

            var (isValid, errorResult) = await validator.ValidateRequestAsync(command, ct);
            if (!isValid) return errorResult!;

            var result = await handler.Handle(command, ct);
            return result.IsSuccess
                ? Results.Ok(ApiResponse<UserResponse>.Ok(result.Value!))
                : Results.BadRequest(ApiResponse<UserResponse>.Fail(result.Error!));
        })
        .WithName("AssignRoleToUser")
        .WithTags("Roles")
        .RequirePermission(Permissions.Roles.Manage);
    }
}
