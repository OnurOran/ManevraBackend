using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using MyApp.Api.Domain.Entities;
using MyApp.Api.Infrastructure.Persistence;

namespace MyApp.Api.Common.Authorization;

public class PermissionSyncService : IHostedService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<PermissionSyncService> _logger;

    public PermissionSyncService(IServiceScopeFactory scopeFactory, ILogger<PermissionSyncService> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        try
        {
            using var scope = _scopeFactory.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole<Guid>>>();

            await SyncPermissionsAsync(db, cancellationToken);
            await EnsureAdminRoleAsync(db, roleManager, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "PermissionSyncService failed during startup. The application will continue, but permissions may be out of sync.");
        }
    }

    private async Task SyncPermissionsAsync(AppDbContext db, CancellationToken ct)
    {
        var defined = Permissions.GetAll().ToHashSet();
        var existing = await db.Permissions.Select(p => p.Name).ToHashSetAsync(ct);

        var toAdd = defined.Except(existing).ToList();
        if (toAdd.Count == 0) return;

        foreach (var name in toAdd)
            db.Permissions.Add(Permission.Create(name));

        try
        {
            await db.SaveChangesAsync(ct);
            _logger.LogInformation("PermissionSyncService: added {Count} new permission(s): {Names}",
                toAdd.Count, string.Join(", ", toAdd));
        }
        catch (Microsoft.EntityFrameworkCore.DbUpdateException ex)
        {
            // Another instance already inserted the same permissions concurrently — safe to ignore.
            _logger.LogWarning(ex, "PermissionSyncService: concurrent insert conflict, permissions already synced by another instance.");
        }
    }

    private async Task EnsureAdminRoleAsync(AppDbContext db, RoleManager<IdentityRole<Guid>> roleManager, CancellationToken ct)
    {
        const string adminRoleName = Permissions.SuperAdminRole;

        if (!await roleManager.RoleExistsAsync(adminRoleName))
        {
            await roleManager.CreateAsync(new IdentityRole<Guid>(adminRoleName));
            _logger.LogInformation("PermissionSyncService: created '{Role}' role", adminRoleName);
        }

        var adminRole = await roleManager.FindByNameAsync(adminRoleName);
        if (adminRole is null) return;

        var allPermissions = await db.Permissions.ToListAsync(ct);
        var assignedIds = await db.RolePermissions
            .Where(rp => rp.RoleId == adminRole.Id)
            .Select(rp => rp.PermissionId)
            .ToHashSetAsync(ct);

        var toAssign = allPermissions.Where(p => !assignedIds.Contains(p.Id)).ToList();
        if (toAssign.Count == 0) return;

        foreach (var permission in toAssign)
            db.RolePermissions.Add(RolePermission.Create(adminRole.Id, permission.Id));

        await db.SaveChangesAsync(ct);
        _logger.LogInformation("PermissionSyncService: assigned {Count} permission(s) to '{Role}' role",
            toAssign.Count, adminRoleName);
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
