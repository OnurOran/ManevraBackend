using MyApp.Api.Common.Behaviors;
using MyApp.Api.Contracts.Roles;

namespace MyApp.Api.Features.Roles.CreateRole;

public class CreateRoleCommand : ICommand<RoleResponse>
{
    public string Name { get; set; } = string.Empty;
}
