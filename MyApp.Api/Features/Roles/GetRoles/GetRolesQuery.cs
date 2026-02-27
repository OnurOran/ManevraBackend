using MyApp.Api.Common.Behaviors;
using MyApp.Api.Contracts.Roles;

namespace MyApp.Api.Features.Roles.GetRoles;

public class GetRolesQuery : IQuery<IEnumerable<RoleResponse>> { }
