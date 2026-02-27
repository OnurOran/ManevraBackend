using Microsoft.AspNetCore.Identity;
using MyApp.Api.Common.Behaviors;
using MyApp.Api.Common.Exceptions;
using MyApp.Api.Common.Models;
using MyApp.Api.Domain.Entities;
using MyApp.Api.Contracts.Users;

namespace MyApp.Api.Features.Users.UpdateProfile;

public class UpdateProfileHandler : ICommandHandler<UpdateProfileCommand, UserResponse>
{
    private readonly UserManager<ApplicationUser> _userManager;

    public UpdateProfileHandler(UserManager<ApplicationUser> userManager)
    {
        _userManager = userManager;
    }

    public async Task<Result<UserResponse>> Handle(UpdateProfileCommand command, CancellationToken cancellationToken)
    {
        var user = await _userManager.FindByIdAsync(command.UserId.ToString());
        if (user is null)
            return Result<UserResponse>.Failure("User not found.");

        user.UpdateProfile(command.FirstName, command.LastName, command.AvatarUrl);
        var result = await _userManager.UpdateAsync(user);

        if (!result.Succeeded)
        {
            var errors = string.Join("; ", result.Errors.Select(e => e.Description));
            return Result<UserResponse>.Failure(errors);
        }

        return Result<UserResponse>.Success(UserMapper.ToResponse(user));
    }
}
