using Microsoft.AspNetCore.Identity;
using MyApp.Api.Common.Authorization;
using MyApp.Api.Common.Behaviors;
using MyApp.Api.Common.Exceptions;
using MyApp.Api.Common.Models;

namespace MyApp.Api.Features.Roles.DeleteRole;

public class DeleteRoleHandler : ICommandHandler<DeleteRoleCommand, bool>
{
    private readonly RoleManager<IdentityRole<Guid>> _roleManager;

    public DeleteRoleHandler(RoleManager<IdentityRole<Guid>> roleManager)
    {
        _roleManager = roleManager;
    }

    public async Task<Result<bool>> Handle(DeleteRoleCommand command, CancellationToken cancellationToken)
    {
        var role = await _roleManager.FindByIdAsync(command.RoleId.ToString());
        if (role is null)
            return Result<bool>.Failure("Role not found.");

        if (string.Equals(role.Name, Permissions.AdminRole, StringComparison.OrdinalIgnoreCase))
            return Result<bool>.Failure("The Admin role cannot be deleted.");

        var result = await _roleManager.DeleteAsync(role);
        return result.Succeeded
            ? Result<bool>.Success(true)
            : Result<bool>.Failure(string.Join("; ", result.Errors.Select(e => e.Description)));
    }
}
