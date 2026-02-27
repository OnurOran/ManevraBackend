using MyApp.Api.Common.Behaviors;
using MyApp.Api.Contracts.Users;

namespace MyApp.Api.Features.Users.RefreshToken;

public class RefreshTokenCommand : ICommand<AuthResponse>
{
    public string Token { get; set; } = string.Empty;
}
