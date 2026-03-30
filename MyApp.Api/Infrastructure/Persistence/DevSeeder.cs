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
        var roles = new[]
        {
            Permissions.SuperAdminRole,
            Permissions.AdminRole,
            Permissions.KumandaMerkeziRole,
            Permissions.ManevraciRole,
            Permissions.HatVardiyaAmiriRole,
            Permissions.SefRole,
            Permissions.AtolyePersoneliRole,
        };
        foreach (var role in roles)
        {
            if (!await roleManager.RoleExistsAsync(role))
                await roleManager.CreateAsync(new IdentityRole<Guid>(role));
        }

        // ── Assign permissions to Admin role ──────────────────────────────
        await AssignPermissionsToRoleAsync(db, roleManager, Permissions.AdminRole, new[]
        {
            // Admin panel
            Permissions.Users.View,
            Permissions.Users.Create,
            Permissions.Users.Edit,
            Permissions.Users.Delete,
            Permissions.Roles.View,
            Permissions.Roles.Manage,
            // General operational
            Permissions.Manevra.View,
            Permissions.Manevra.Edit,
            Permissions.Manevra.Approve,
            Permissions.Wagons.View,
            Permissions.Wagons.Create,
            Permissions.Wagons.Edit,
            Permissions.Wagons.Delete,
            Permissions.Cleanup.View,
            Permissions.Cleanup.Edit,
            Permissions.WeeklyMaintenance.View,
            Permissions.WeeklyMaintenance.Edit,
            Permissions.Field.View,
            // Vagon Listesi: Ozellikler, Surukle, Goruntule
            Permissions.VagonListesi.Ozellikler,
            Permissions.VagonListesi.Surukle,
            Permissions.VagonListesi.Goruntule,
            // Temizlik: Goruntule, Yaz
            Permissions.Temizlik.Goruntule,
            Permissions.Temizlik.Yaz,
            // Cari Hat: DiziyiBoz, Surukle, Goruntule
            Permissions.CariHat.DiziyiBoz,
            Permissions.CariHat.Surukle,
            Permissions.CariHat.Goruntule,
            // Bas Makas: ALL
            Permissions.BasMakas.Servis,
            Permissions.BasMakas.CalismaYapilacak,
            Permissions.BasMakas.ServiseHazir,
            Permissions.BasMakas.DiziyiBoz,
            Permissions.BasMakas.Ozellikler,
            Permissions.BasMakas.Surukle,
            Permissions.BasMakas.Goruntule,
            Permissions.BasMakas.Yaz,
            // Itfaiye Yonu: ALL
            Permissions.ItfaiyeYonu.Servis,
            Permissions.ItfaiyeYonu.CalismaYapilacak,
            Permissions.ItfaiyeYonu.ServiseHazir,
            Permissions.ItfaiyeYonu.DiziyiBoz,
            Permissions.ItfaiyeYonu.Ozellikler,
            Permissions.ItfaiyeYonu.Surukle,
            Permissions.ItfaiyeYonu.Goruntule,
            Permissions.ItfaiyeYonu.Yaz,
            // Atolye Yollari: ALL except Yaz
            Permissions.AtolyeYollari.Servis,
            Permissions.AtolyeYollari.CalismaYapilacak,
            Permissions.AtolyeYollari.ServiseHazir,
            Permissions.AtolyeYollari.DiziyiBoz,
            Permissions.AtolyeYollari.Ozellikler,
            Permissions.AtolyeYollari.Surukle,
            Permissions.AtolyeYollari.Goruntule,
            // Itfaiye Taraf: ALL except Yaz
            Permissions.ItfaiyeTaraf.Servis,
            Permissions.ItfaiyeTaraf.CalismaYapilacak,
            Permissions.ItfaiyeTaraf.ServiseHazir,
            Permissions.ItfaiyeTaraf.DiziyiBoz,
            Permissions.ItfaiyeTaraf.Ozellikler,
            Permissions.ItfaiyeTaraf.Surukle,
            Permissions.ItfaiyeTaraf.Goruntule,
            // Yikama Taraf: ALL except Yaz
            Permissions.YikamaTaraf.Servis,
            Permissions.YikamaTaraf.CalismaYapilacak,
            Permissions.YikamaTaraf.ServiseHazir,
            Permissions.YikamaTaraf.DiziyiBoz,
            Permissions.YikamaTaraf.Ozellikler,
            Permissions.YikamaTaraf.Surukle,
            Permissions.YikamaTaraf.Goruntule,
            // Servis Disi: ALL except Yaz
            Permissions.ServisDisi.Servis,
            Permissions.ServisDisi.CalismaYapilacak,
            Permissions.ServisDisi.ServiseHazir,
            Permissions.ServisDisi.DiziyiBoz,
            Permissions.ServisDisi.Ozellikler,
            Permissions.ServisDisi.Surukle,
            Permissions.ServisDisi.Goruntule,
            // Haftalik Bakim: ALL except Yaz
            Permissions.HaftalikBakim.Servis,
            Permissions.HaftalikBakim.CalismaYapilacak,
            Permissions.HaftalikBakim.ServiseHazir,
            Permissions.HaftalikBakim.DiziyiBoz,
            Permissions.HaftalikBakim.Ozellikler,
            Permissions.HaftalikBakim.Surukle,
            Permissions.HaftalikBakim.Goruntule,
            // Haftalik Torna: ALL except Yaz
            Permissions.HaftalikTorna.Servis,
            Permissions.HaftalikTorna.CalismaYapilacak,
            Permissions.HaftalikTorna.ServiseHazir,
            Permissions.HaftalikTorna.DiziyiBoz,
            Permissions.HaftalikTorna.Ozellikler,
            Permissions.HaftalikTorna.Surukle,
            Permissions.HaftalikTorna.Goruntule,
        }, logger);

        // ── Assign permissions to Kumanda Merkezi role ────────────────────
        await AssignPermissionsToRoleAsync(db, roleManager, Permissions.KumandaMerkeziRole, new[]
        {
            // General operational
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
            Permissions.Field.View,
            // Vagon Listesi: Ozellikler, Surukle, Goruntule
            Permissions.VagonListesi.Ozellikler,
            Permissions.VagonListesi.Surukle,
            Permissions.VagonListesi.Goruntule,
            // Temizlik: Goruntule, Yaz
            Permissions.Temizlik.Goruntule,
            Permissions.Temizlik.Yaz,
            // Cari Hat: DiziyiBoz, Surukle, Goruntule
            Permissions.CariHat.DiziyiBoz,
            Permissions.CariHat.Surukle,
            Permissions.CariHat.Goruntule,
            // Bas Makas: Servis, CalismaYapilacak, DiziyiBoz, Ozellikler, Surukle, Goruntule, Yaz
            Permissions.BasMakas.Servis,
            Permissions.BasMakas.CalismaYapilacak,
            Permissions.BasMakas.DiziyiBoz,
            Permissions.BasMakas.Ozellikler,
            Permissions.BasMakas.Surukle,
            Permissions.BasMakas.Goruntule,
            Permissions.BasMakas.Yaz,
            // Itfaiye Yonu: Servis, CalismaYapilacak, DiziyiBoz, Ozellikler, Surukle, Goruntule, Yaz
            Permissions.ItfaiyeYonu.Servis,
            Permissions.ItfaiyeYonu.CalismaYapilacak,
            Permissions.ItfaiyeYonu.DiziyiBoz,
            Permissions.ItfaiyeYonu.Ozellikler,
            Permissions.ItfaiyeYonu.Surukle,
            Permissions.ItfaiyeYonu.Goruntule,
            Permissions.ItfaiyeYonu.Yaz,
            // Atolye Yollari: Servis, CalismaYapilacak, DiziyiBoz, Ozellikler, Surukle, Goruntule
            Permissions.AtolyeYollari.Servis,
            Permissions.AtolyeYollari.CalismaYapilacak,
            Permissions.AtolyeYollari.DiziyiBoz,
            Permissions.AtolyeYollari.Ozellikler,
            Permissions.AtolyeYollari.Surukle,
            Permissions.AtolyeYollari.Goruntule,
            // Itfaiye Taraf: Servis, CalismaYapilacak, DiziyiBoz, Ozellikler, Surukle, Goruntule
            Permissions.ItfaiyeTaraf.Servis,
            Permissions.ItfaiyeTaraf.CalismaYapilacak,
            Permissions.ItfaiyeTaraf.DiziyiBoz,
            Permissions.ItfaiyeTaraf.Ozellikler,
            Permissions.ItfaiyeTaraf.Surukle,
            Permissions.ItfaiyeTaraf.Goruntule,
            // Yikama Taraf: Servis, CalismaYapilacak, DiziyiBoz, Ozellikler, Surukle, Goruntule
            Permissions.YikamaTaraf.Servis,
            Permissions.YikamaTaraf.CalismaYapilacak,
            Permissions.YikamaTaraf.DiziyiBoz,
            Permissions.YikamaTaraf.Ozellikler,
            Permissions.YikamaTaraf.Surukle,
            Permissions.YikamaTaraf.Goruntule,
            // Servis Disi: Servis, CalismaYapilacak, DiziyiBoz, Ozellikler, Surukle, Goruntule
            Permissions.ServisDisi.Servis,
            Permissions.ServisDisi.CalismaYapilacak,
            Permissions.ServisDisi.DiziyiBoz,
            Permissions.ServisDisi.Ozellikler,
            Permissions.ServisDisi.Surukle,
            Permissions.ServisDisi.Goruntule,
            // Haftalik Bakim: Servis, CalismaYapilacak, DiziyiBoz, Ozellikler, Surukle, Goruntule
            Permissions.HaftalikBakim.Servis,
            Permissions.HaftalikBakim.CalismaYapilacak,
            Permissions.HaftalikBakim.DiziyiBoz,
            Permissions.HaftalikBakim.Ozellikler,
            Permissions.HaftalikBakim.Surukle,
            Permissions.HaftalikBakim.Goruntule,
            // Haftalik Torna: Servis, CalismaYapilacak, DiziyiBoz, Ozellikler, Surukle, Goruntule
            Permissions.HaftalikTorna.Servis,
            Permissions.HaftalikTorna.CalismaYapilacak,
            Permissions.HaftalikTorna.DiziyiBoz,
            Permissions.HaftalikTorna.Ozellikler,
            Permissions.HaftalikTorna.Surukle,
            Permissions.HaftalikTorna.Goruntule,
        }, logger);

        // ── Assign permissions to Manevraci role ──────────────────────────
        await AssignPermissionsToRoleAsync(db, roleManager, Permissions.ManevraciRole, new[]
        {
            Permissions.Manevra.View,
            Permissions.Field.View,
            Permissions.Wagons.View,
            Permissions.Cleanup.View,
            Permissions.WeeklyMaintenance.View,
            // All zones: Goruntule
            Permissions.VagonListesi.Goruntule,
            Permissions.Temizlik.Goruntule,
            Permissions.CariHat.Goruntule,
            Permissions.BasMakas.Goruntule,
            Permissions.ItfaiyeYonu.Goruntule,
            Permissions.AtolyeYollari.Goruntule,
            Permissions.ItfaiyeTaraf.Goruntule,
            Permissions.YikamaTaraf.Goruntule,
            Permissions.ServisDisi.Goruntule,
            Permissions.HaftalikBakim.Goruntule,
            Permissions.HaftalikTorna.Goruntule,
            // Itfaiye Yonu: Yaz
            Permissions.ItfaiyeYonu.Yaz,
        }, logger);

        // ── Assign permissions to Hat Vardiya Amiri role ──────────────────
        await AssignPermissionsToRoleAsync(db, roleManager, Permissions.HatVardiyaAmiriRole, new[]
        {
            Permissions.Manevra.View,
            Permissions.Field.View,
            Permissions.Wagons.View,
            Permissions.Cleanup.View,
            Permissions.WeeklyMaintenance.View,
            // All zones: Goruntule
            Permissions.VagonListesi.Goruntule,
            Permissions.Temizlik.Goruntule,
            Permissions.CariHat.Goruntule,
            Permissions.BasMakas.Goruntule,
            Permissions.ItfaiyeYonu.Goruntule,
            Permissions.AtolyeYollari.Goruntule,
            Permissions.ItfaiyeTaraf.Goruntule,
            Permissions.YikamaTaraf.Goruntule,
            Permissions.ServisDisi.Goruntule,
            Permissions.HaftalikBakim.Goruntule,
            Permissions.HaftalikTorna.Goruntule,
            // Itfaiye Yonu: Yaz
            Permissions.ItfaiyeYonu.Yaz,
        }, logger);

        // ── Assign permissions to Sef role ────────────────────────────────
        await AssignPermissionsToRoleAsync(db, roleManager, Permissions.SefRole, new[]
        {
            Permissions.Manevra.View,
            Permissions.Field.View,
            Permissions.Wagons.View,
            Permissions.Cleanup.View,
            Permissions.WeeklyMaintenance.View,
            // All zones: Goruntule
            Permissions.VagonListesi.Goruntule,
            Permissions.Temizlik.Goruntule,
            Permissions.CariHat.Goruntule,
            Permissions.BasMakas.Goruntule,
            Permissions.ItfaiyeYonu.Goruntule,
            Permissions.AtolyeYollari.Goruntule,
            Permissions.ItfaiyeTaraf.Goruntule,
            Permissions.YikamaTaraf.Goruntule,
            Permissions.ServisDisi.Goruntule,
            Permissions.HaftalikBakim.Goruntule,
            Permissions.HaftalikTorna.Goruntule,
            // Itfaiye Yonu: Yaz
            Permissions.ItfaiyeYonu.Yaz,
        }, logger);

        // ── Assign permissions to Atolye Personeli role ───────────────────
        await AssignPermissionsToRoleAsync(db, roleManager, Permissions.AtolyePersoneliRole, new[]
        {
            Permissions.Manevra.View,
            Permissions.Manevra.Edit,
            Permissions.Field.View,
            Permissions.Wagons.View,
            Permissions.Wagons.Edit,
            Permissions.Cleanup.View,
            Permissions.WeeklyMaintenance.View,
            // Vagon Listesi: Surukle, Goruntule
            Permissions.VagonListesi.Surukle,
            Permissions.VagonListesi.Goruntule,
            // Temizlik: Goruntule
            Permissions.Temizlik.Goruntule,
            // Cari Hat: Goruntule
            Permissions.CariHat.Goruntule,
            // Bas Makas: ServiseHazir, Goruntule
            Permissions.BasMakas.ServiseHazir,
            Permissions.BasMakas.Goruntule,
            // Itfaiye Yonu: ServiseHazir, Goruntule, Yaz
            Permissions.ItfaiyeYonu.ServiseHazir,
            Permissions.ItfaiyeYonu.Goruntule,
            Permissions.ItfaiyeYonu.Yaz,
            // Atolye Yollari: ServiseHazir, Surukle, Goruntule
            Permissions.AtolyeYollari.ServiseHazir,
            Permissions.AtolyeYollari.Surukle,
            Permissions.AtolyeYollari.Goruntule,
            // Itfaiye Taraf: ServiseHazir, Surukle, Goruntule
            Permissions.ItfaiyeTaraf.ServiseHazir,
            Permissions.ItfaiyeTaraf.Surukle,
            Permissions.ItfaiyeTaraf.Goruntule,
            // Yikama Taraf: ServiseHazir, Surukle, Goruntule
            Permissions.YikamaTaraf.ServiseHazir,
            Permissions.YikamaTaraf.Surukle,
            Permissions.YikamaTaraf.Goruntule,
            // Servis Disi: ServiseHazir, Surukle, Goruntule
            Permissions.ServisDisi.ServiseHazir,
            Permissions.ServisDisi.Surukle,
            Permissions.ServisDisi.Goruntule,
            // Haftalik Bakim: ServiseHazir, Surukle, Goruntule
            Permissions.HaftalikBakim.ServiseHazir,
            Permissions.HaftalikBakim.Surukle,
            Permissions.HaftalikBakim.Goruntule,
            // Haftalik Torna: ServiseHazir, Surukle, Goruntule
            Permissions.HaftalikTorna.ServiseHazir,
            Permissions.HaftalikTorna.Surukle,
            Permissions.HaftalikTorna.Goruntule,
        }, logger);

        // ── Seed users ──────────────────────────────────────────────────────
        var adminEmail = configuration["Seed:AdminEmail"];
        var adminPassword = configuration["Seed:AdminPassword"];

        if (!string.IsNullOrWhiteSpace(adminEmail) && !string.IsNullOrWhiteSpace(adminPassword))
            await EnsureUserAsync(userManager, logger, adminEmail, adminPassword, "Admin", "User", Permissions.SuperAdminRole);

        await EnsureUserAsync(userManager, logger, "kumanda@metro.istanbul", "Kumanda123!", "Kumanda", "Merkezi", Permissions.KumandaMerkeziRole);
        await EnsureUserAsync(userManager, logger, "manevraci@metro.istanbul", "Manevraci123!", "Manevraci", "User", Permissions.ManevraciRole);
        await EnsureUserAsync(userManager, logger, "vardiya@metro.istanbul", "Vardiya123!", "Hat Vardiya", "Amiri", Permissions.HatVardiyaAmiriRole);
        await EnsureUserAsync(userManager, logger, "sef@metro.istanbul", "Sef12345!", "Sef", "User", Permissions.SefRole);
        await EnsureUserAsync(userManager, logger, "atolye@metro.istanbul", "Atolye123!", "Atolye", "Personeli", Permissions.AtolyePersoneliRole);
        await EnsureUserAsync(userManager, logger, "admin@metro.istanbul", "Admin1234!", "Admin", "User", Permissions.AdminRole);
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
