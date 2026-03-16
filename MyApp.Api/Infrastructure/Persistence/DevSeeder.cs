using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using MyApp.Api.Common.Authorization;
using MyApp.Api.Domain.Entities;

namespace MyApp.Api.Infrastructure.Persistence;

/// <summary>
/// Seeds default users and roles in the Development environment.
/// Runs once at startup after migrations; fully idempotent.
/// Configure admin credentials in appsettings.Development.json under "Seed".
/// </summary>
public static class DevSeeder
{
    public static async Task SeedAsync(IServiceProvider services, IConfiguration configuration)
    {
        using var scope = services.CreateScope();
        var logger = scope.ServiceProvider.GetRequiredService<ILoggerFactory>().CreateLogger(nameof(DevSeeder));
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole<Guid>>>();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        // ── Ensure all permissions exist (runs before PermissionSyncService) ─
        var defined = Permissions.GetAll().ToHashSet();
        var existing = await db.Permissions.Select(p => p.Name).ToHashSetAsync();
        var toAdd = defined.Except(existing).ToList();
        foreach (var name in toAdd)
            db.Permissions.Add(Domain.Entities.Permission.Create(name));
        if (toAdd.Count > 0)
            await db.SaveChangesAsync();

        // ── Ensure roles exist ──────────────────────────────────────────────
        var roles = new[] { Permissions.AdminRole, Permissions.OfficeRole, Permissions.FieldRole };
        foreach (var role in roles)
        {
            if (!await roleManager.RoleExistsAsync(role))
                await roleManager.CreateAsync(new IdentityRole<Guid>(role));
        }

        // ── Assign permissions to Office role ───────────────────────────────
        await AssignPermissionsToRoleAsync(db, roleManager, Permissions.OfficeRole, new[]
        {
            Permissions.Manevra.View,
            Permissions.Manevra.Edit,
            Permissions.Manevra.Approve,
            Permissions.Wagons.View,
            Permissions.Wagons.Create,
            Permissions.Wagons.Edit,
            Permissions.Cleanup.View,
            Permissions.Cleanup.Edit,
            Permissions.WeeklyMaintenance.View,
            Permissions.WeeklyMaintenance.Edit,
        }, logger);

        // ── Assign permissions to Field role ────────────────────────────────
        await AssignPermissionsToRoleAsync(db, roleManager, Permissions.FieldRole, new[]
        {
            Permissions.Field.View,
            Permissions.WeeklyMaintenance.View,
        }, logger);

        // ── Seed users ──────────────────────────────────────────────────────
        var adminEmail = configuration["Seed:AdminEmail"];
        var adminPassword = configuration["Seed:AdminPassword"];

        if (!string.IsNullOrWhiteSpace(adminEmail) && !string.IsNullOrWhiteSpace(adminPassword))
            await EnsureUserAsync(userManager, logger, adminEmail, adminPassword, "Admin", "User", Permissions.AdminRole);

        await EnsureUserAsync(userManager, logger, "office@example.com", "Office123!", "Office", "User", Permissions.OfficeRole);
        await EnsureUserAsync(userManager, logger, "field@example.com", "Field123!", "Field", "User", Permissions.FieldRole);
    }

    private static async Task EnsureUserAsync(
        UserManager<ApplicationUser> userManager,
        ILogger logger,
        string email,
        string password,
        string firstName,
        string lastName,
        string role)
    {
        if (await userManager.FindByEmailAsync(email) is not null)
        {
            logger.LogDebug("DevSeeder: user '{Email}' already exists. Skipping.", email);
            return;
        }

        var user = ApplicationUser.Create(email, firstName, lastName);
        var result = await userManager.CreateAsync(user, password);

        if (!result.Succeeded)
        {
            var errors = string.Join("; ", result.Errors.Select(e => e.Description));
            logger.LogError("DevSeeder: failed to create user '{Email}': {Errors}", email, errors);
            return;
        }

        await userManager.AddToRoleAsync(user, role);
        logger.LogInformation("DevSeeder: created user '{Email}' with role '{Role}'.", email, role);
    }

    private static async Task AssignPermissionsToRoleAsync(
        AppDbContext db,
        RoleManager<IdentityRole<Guid>> roleManager,
        string roleName,
        string[] permissionNames,
        ILogger logger)
    {
        var role = await roleManager.FindByNameAsync(roleName);
        if (role is null) return;

        var permissions = await db.Permissions
            .Where(p => permissionNames.Contains(p.Name))
            .ToListAsync();

        var existingPermissionIds = await db.RolePermissions
            .Where(rp => rp.RoleId == role.Id)
            .Select(rp => rp.PermissionId)
            .ToListAsync();

        var toAdd = permissions
            .Where(p => !existingPermissionIds.Contains(p.Id))
            .ToList();

        foreach (var perm in toAdd)
        {
            db.RolePermissions.Add(Domain.Entities.RolePermission.Create(role.Id, perm.Id));
        }

        if (toAdd.Count > 0)
        {
            await db.SaveChangesAsync();
            logger.LogInformation("DevSeeder: assigned {Count} permission(s) to '{Role}' role.", toAdd.Count, roleName);
        }
    }
}
