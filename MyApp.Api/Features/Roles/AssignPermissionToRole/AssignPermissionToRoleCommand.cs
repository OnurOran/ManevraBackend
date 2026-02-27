using MyApp.Api.Common.Behaviors;
using MyApp.Api.Contracts.Roles;

namespace MyApp.Api.Features.Roles.AssignPermissionToRole;

public class AssignPermissionToRoleCommand : ICommand<RoleResponse>
{
    public Guid RoleId { get; set; }
    public Guid PermissionId { get; set; }
}
