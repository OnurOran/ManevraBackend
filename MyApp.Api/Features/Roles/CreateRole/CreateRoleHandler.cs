using Microsoft.AspNetCore.Identity;
using MyApp.Api.Common.Behaviors;
using MyApp.Api.Common.Models;
using MyApp.Api.Contracts.Roles;

namespace MyApp.Api.Features.Roles.CreateRole;

public class CreateRoleHandler : ICommandHandler<CreateRoleCommand, RoleResponse>
{
    private readonly RoleManager<IdentityRole<Guid>> _roleManager;

    public CreateRoleHandler(RoleManager<IdentityRole<Guid>> roleManager)
    {
        _roleManager = roleManager;
    }

    public async Task<Result<RoleResponse>> Handle(CreateRoleCommand command, CancellationToken cancellationToken)
    {
        if (await _roleManager.RoleExistsAsync(command.Name))
            return Result<RoleResponse>.Failure($"Role '{command.Name}' already exists.");

        var role = new IdentityRole<Guid>(command.Name);
        var result = await _roleManager.CreateAsync(role);

        if (!result.Succeeded)
            return Result<RoleResponse>.Failure(string.Join("; ", result.Errors.Select(e => e.Description)));

        return Result<RoleResponse>.Success(new RoleResponse { Id = role.Id, Name = role.Name! });
    }
}
