using Microsoft.AspNetCore.Identity;
using MyApp.Api.Common.Behaviors;
using MyApp.Api.Common.Models;
using MyApp.Api.Domain.Entities;
using MyApp.Api.Features.Users;
using MyApp.Api.Contracts.Users;

namespace MyApp.Api.Features.Roles.AssignRoleToUser;

public class AssignRoleToUserHandler : ICommandHandler<AssignRoleToUserCommand, UserResponse>
{
    private readonly UserManager<ApplicationUser> _userManager;

    public AssignRoleToUserHandler(UserManager<ApplicationUser> userManager)
    {
        _userManager = userManager;
    }

    public async Task<Result<UserResponse>> Handle(AssignRoleToUserCommand command, CancellationToken cancellationToken)
    {
        var user = await _userManager.FindByIdAsync(command.UserId.ToString());
        if (user is null)
            return Result<UserResponse>.Failure("User not found.");

        if (await _userManager.IsInRoleAsync(user, command.RoleName))
            return Result<UserResponse>.Failure($"User already has the '{command.RoleName}' role.");

        var result = await _userManager.AddToRoleAsync(user, command.RoleName);
        return result.Succeeded
            ? Result<UserResponse>.Success(UserMapper.ToResponse(user))
            : Result<UserResponse>.Failure(string.Join("; ", result.Errors.Select(e => e.Description)));
    }
}
