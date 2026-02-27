using MyApp.Api.Common.Behaviors;
using MyApp.Api.Contracts.Users;

namespace MyApp.Api.Features.Users.LoginUser;

public class LoginUserCommand : ICommand<AuthResponse>
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}
