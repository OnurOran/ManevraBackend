using MyApp.Api.Common.Behaviors;
using MyApp.Api.Common.Models;
using MyApp.Api.Domain.Entities;
using MyApp.Api.Contracts.Users;
using Microsoft.AspNetCore.Identity;

namespace MyApp.Api.Features.Users.RegisterUser;

public class RegisterUserHandler : ICommandHandler<RegisterUserCommand, UserResponse>
{
    private readonly UserManager<ApplicationUser> _userManager;

    public RegisterUserHandler(UserManager<ApplicationUser> userManager)
    {
        _userManager = userManager;
    }

    public async Task<Result<UserResponse>> Handle(RegisterUserCommand command, CancellationToken cancellationToken)
    {
        var existing = await _userManager.FindByEmailAsync(command.Email);
        if (existing is not null)
            return Result<UserResponse>.Failure("A user with this email already exists.");

        var user = ApplicationUser.Create(command.Email, command.FirstName, command.LastName);
        var result = await _userManager.CreateAsync(user, command.Password);

        if (!result.Succeeded)
        {
            var errors = string.Join("; ", result.Errors.Select(e => e.Description));
            return Result<UserResponse>.Failure(errors);
        }

        return Result<UserResponse>.Success(UserMapper.ToResponse(user));
    }
}
