using MyApp.Api.Common.Behaviors;

namespace MyApp.Api.Features.Roles.RemovePermissionFromRole;

public class RemovePermissionFromRoleCommand : ICommand<bool>
{
    public Guid RoleId { get; set; }
    public Guid PermissionId { get; set; }
}
