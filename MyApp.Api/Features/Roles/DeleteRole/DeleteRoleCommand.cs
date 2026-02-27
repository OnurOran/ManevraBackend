using MyApp.Api.Common.Behaviors;

namespace MyApp.Api.Features.Roles.DeleteRole;

public class DeleteRoleCommand : ICommand<bool>
{
    public Guid RoleId { get; set; }
}
