using MyApp.Api.Common.Behaviors;
using MyApp.Api.Contracts.Users;

namespace MyApp.Api.Features.Roles.AssignRoleToUser;

public class AssignRoleToUserCommand : ICommand<UserResponse>
{
    public Guid UserId { get; set; }
    public string RoleName { get; set; } = string.Empty;
}
