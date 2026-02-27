using Microsoft.EntityFrameworkCore;
using MyApp.Api.Common.Behaviors;
using MyApp.Api.Common.Models;
using MyApp.Api.Domain.Entities;
using MyApp.Api.Infrastructure.Persistence;
using MyApp.Api.Contracts.Roles;

namespace MyApp.Api.Features.Roles.AssignPermissionToRole;

public class AssignPermissionToRoleHandler : ICommandHandler<AssignPermissionToRoleCommand, RoleResponse>
{
    private readonly AppDbContext _db;

    public AssignPermissionToRoleHandler(AppDbContext db)
    {
        _db = db;
    }

    public async Task<Result<RoleResponse>> Handle(AssignPermissionToRoleCommand command, CancellationToken cancellationToken)
    {
        var role = await _db.Roles.FindAsync([command.RoleId], cancellationToken);
        if (role is null)
            return Result<RoleResponse>.Failure("Role not found.");

        var permission = await _db.Permissions.FindAsync([command.PermissionId], cancellationToken);
        if (permission is null)
            return Result<RoleResponse>.Failure("Permission not found.");

        var exists = await _db.RolePermissions
            .AnyAsync(rp => rp.RoleId == command.RoleId && rp.PermissionId == command.PermissionId, cancellationToken);
        if (exists)
            return Result<RoleResponse>.Failure("Permission is already assigned to this role.");

        _db.RolePermissions.Add(RolePermission.Create(command.RoleId, command.PermissionId));
        await _db.SaveChangesAsync(cancellationToken);

        var allPermissions = await _db.RolePermissions
            .Where(rp => rp.RoleId == command.RoleId)
            .Include(rp => rp.Permission)
            .Select(rp => new PermissionResponse { Id = rp.PermissionId, Name = rp.Permission.Name })
            .ToListAsync(cancellationToken);

        return Result<RoleResponse>.Success(new RoleResponse
        {
            Id = role.Id,
            Name = role.Name!,
            Permissions = allPermissions
        });
    }
}
