using MyApp.Api.Common.Behaviors;
using MyApp.Api.Contracts.Users;

namespace MyApp.Api.Features.Users.UpdateProfile;

public class UpdateProfileCommand : ICommand<UserResponse>
{
    public Guid UserId { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string? AvatarUrl { get; set; }
}
