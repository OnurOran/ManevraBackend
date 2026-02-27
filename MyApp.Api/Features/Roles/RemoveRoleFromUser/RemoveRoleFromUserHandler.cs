using Microsoft.AspNetCore.Identity;
using MyApp.Api.Common.Behaviors;
using MyApp.Api.Common.Models;
using MyApp.Api.Domain.Entities;

namespace MyApp.Api.Features.Roles.RemoveRoleFromUser;

public class RemoveRoleFromUserHandler : ICommandHandler<RemoveRoleFromUserCommand, bool>
{
    private readonly UserManager<ApplicationUser> _userManager;

    public RemoveRoleFromUserHandler(UserManager<ApplicationUser> userManager)
    {
        _userManager = userManager;
    }

    public async Task<Result<bool>> Handle(RemoveRoleFromUserCommand command, CancellationToken cancellationToken)
    {
        var user = await _userManager.FindByIdAsync(command.UserId.ToString());
        if (user is null)
            return Result<bool>.Failure("User not found.");

        if (!await _userManager.IsInRoleAsync(user, command.RoleName))
            return Result<bool>.Failure($"User does not have the '{command.RoleName}' role.");

        var result = await _userManager.RemoveFromRoleAsync(user, command.RoleName);
        return result.Succeeded
            ? Result<bool>.Success(true)
            : Result<bool>.Failure(string.Join("; ", result.Errors.Select(e => e.Description)));
    }
}
