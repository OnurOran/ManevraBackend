using MyApp.Api.Common.Behaviors;

namespace MyApp.Api.Features.Roles.RemoveRoleFromUser;

public class RemoveRoleFromUserCommand : ICommand<bool>
{
    public Guid UserId { get; set; }
    public string RoleName { get; set; } = string.Empty;
}
