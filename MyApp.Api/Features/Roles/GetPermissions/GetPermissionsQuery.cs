using MyApp.Api.Common.Behaviors;
using MyApp.Api.Contracts.Roles;

namespace MyApp.Api.Features.Roles.GetPermissions;

public class GetPermissionsQuery : IQuery<IEnumerable<PermissionResponse>> { }
