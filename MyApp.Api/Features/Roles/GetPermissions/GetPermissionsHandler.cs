using Microsoft.EntityFrameworkCore;
using MyApp.Api.Common.Behaviors;
using MyApp.Api.Common.Models;
using MyApp.Api.Infrastructure.Persistence;
using MyApp.Api.Contracts.Roles;

namespace MyApp.Api.Features.Roles.GetPermissions;

public class GetPermissionsHandler : IQueryHandler<GetPermissionsQuery, IEnumerable<PermissionResponse>>
{
    private readonly AppDbContext _db;

    public GetPermissionsHandler(AppDbContext db)
    {
        _db = db;
    }

    public async Task<Result<IEnumerable<PermissionResponse>>> Handle(GetPermissionsQuery query, CancellationToken cancellationToken)
    {
        var permissions = await _db.Permissions
            .OrderBy(p => p.Name)
            .Select(p => new PermissionResponse { Id = p.Id, Name = p.Name })
            .ToListAsync(cancellationToken);

        return Result<IEnumerable<PermissionResponse>>.Success(permissions);
    }
}
