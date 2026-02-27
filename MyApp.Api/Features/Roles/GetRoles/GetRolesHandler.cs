using Microsoft.EntityFrameworkCore;
using MyApp.Api.Common.Behaviors;
using MyApp.Api.Common.Models;
using MyApp.Api.Infrastructure.Persistence;
using MyApp.Api.Contracts.Roles;

namespace MyApp.Api.Features.Roles.GetRoles;

public class GetRolesHandler : IQueryHandler<GetRolesQuery, IEnumerable<RoleResponse>>
{
    private readonly AppDbContext _db;

    public GetRolesHandler(AppDbContext db)
    {
        _db = db;
    }

    public async Task<Result<IEnumerable<RoleResponse>>> Handle(GetRolesQuery query, CancellationToken cancellationToken)
    {
        var roles = await _db.Roles
            .OrderBy(r => r.Name)
            .ToListAsync(cancellationToken);

        var roleIds = roles.Select(r => r.Id).ToList();

        var permissionsByRole = await _db.RolePermissions
            .Where(rp => roleIds.Contains(rp.RoleId))
            .Select(rp => new { rp.RoleId, rp.PermissionId, rp.Permission.Name })
            .OrderBy(rp => rp.Name)
            .ToListAsync(cancellationToken);

        var grouped = permissionsByRole
            .GroupBy(rp => rp.RoleId)
            .ToDictionary(
                g => g.Key,
                g => g.Select(rp => new PermissionResponse { Id = rp.PermissionId, Name = rp.Name }).ToList());

        var result = roles.Select(r => new RoleResponse
        {
            Id = r.Id,
            Name = r.Name!,
            Permissions = grouped.GetValueOrDefault(r.Id, [])
        });

        return Result<IEnumerable<RoleResponse>>.Success(result);
    }
}
