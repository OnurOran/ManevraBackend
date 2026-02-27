using FluentValidation;
using MyApp.Api.Common.Authorization;
using MyApp.Api.Common.Behaviors;
using MyApp.Api.Common.Extensions;
using MyApp.Api.Common.Models;
using MyApp.Api.Contracts.Roles;
using MyApp.Api.Common.Attributes;

namespace MyApp.Api.Features.Roles.CreateRole;

[MapToGroup("roles")]
public static class CreateRoleEndpoint
{
    public static void Map(RouteGroupBuilder group)
    {
        group.MapPost("/", async Task<IResult> (
            CreateRoleRequest request,
            ICommandHandler<CreateRoleCommand, RoleResponse> handler,
            IValidator<CreateRoleCommand> validator,
            CancellationToken ct) =>
        {
            var command = new CreateRoleCommand { Name = request.Name };

            var (isValid, errorResult) = await validator.ValidateRequestAsync(command, ct);
            if (!isValid) return errorResult!;

            var result = await handler.Handle(command, ct);
            return result.IsSuccess
                ? Results.Created($"/api/v1/roles", ApiResponse<RoleResponse>.Ok(result.Value!))
                : Results.BadRequest(ApiResponse<RoleResponse>.Fail(result.Error!));
        })
        .WithName("CreateRole")
        .WithTags("Roles")
        .RequirePermission(Permissions.Roles.Manage);
    }
}
