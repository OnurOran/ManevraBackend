using Microsoft.EntityFrameworkCore;
using MyApp.Api.Common.Behaviors;
using MyApp.Api.Common.Models;
using MyApp.Api.Infrastructure.Persistence;

namespace MyApp.Api.Features.Roles.RemovePermissionFromRole;

public class RemovePermissionFromRoleHandler : ICommandHandler<RemovePermissionFromRoleCommand, bool>
{
    private readonly AppDbContext _db;

    public RemovePermissionFromRoleHandler(AppDbContext db)
    {
        _db = db;
    }

    public async Task<Result<bool>> Handle(RemovePermissionFromRoleCommand command, CancellationToken cancellationToken)
    {
        var rolePermission = await _db.RolePermissions
            .FirstOrDefaultAsync(rp => rp.RoleId == command.RoleId && rp.PermissionId == command.PermissionId,
                cancellationToken);

        if (rolePermission is null)
            return Result<bool>.Failure("Permission is not assigned to this role.");

        _db.RolePermissions.Remove(rolePermission);
        await _db.SaveChangesAsync(cancellationToken);

        return Result<bool>.Success(true);
    }
}
