using MyApp.Api.Common.Behaviors;
using MyApp.Api.Contracts.Users;

namespace MyApp.Api.Features.Users.RegisterUser;

public class RegisterUserCommand : ICommand<UserResponse>
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
}
