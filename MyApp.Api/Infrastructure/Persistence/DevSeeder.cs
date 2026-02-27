using Microsoft.AspNetCore.Identity;
using MyApp.Api.Common.Authorization;
using MyApp.Api.Domain.Entities;

namespace MyApp.Api.Infrastructure.Persistence;

/// <summary>
/// Seeds a default admin user in the Development environment.
/// Runs once at startup after migrations; fully idempotent.
/// Configure credentials in appsettings.Development.json under "Seed".
/// </summary>
public static class DevSeeder
{
    public static async Task SeedAsync(IServiceProvider services, IConfiguration configuration)
    {
        var email    = configuration["Seed:AdminEmail"];
        var password = configuration["Seed:AdminPassword"];

        using var scope       = services.CreateScope();
        var logger            = scope.ServiceProvider.GetRequiredService<ILoggerFactory>().CreateLogger(nameof(DevSeeder));
        var userManager       = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var roleManager       = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole<Guid>>>();

        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
        {
            logger.LogWarning("DevSeeder: Seed:AdminEmail or Seed:AdminPassword is not configured. Skipping.");
            return;
        }

        // Ensure Admin role exists (PermissionSyncService runs later as a hosted service)
        if (!await roleManager.RoleExistsAsync(Permissions.AdminRole))
            await roleManager.CreateAsync(new IdentityRole<Guid>(Permissions.AdminRole));

        // Skip if user already exists
        if (await userManager.FindByEmailAsync(email) is not null)
        {
            logger.LogDebug("DevSeeder: admin user '{Email}' already exists. Skipping.", email);
            return;
        }

        var user = ApplicationUser.Create(email, "Admin", "User");
        var result = await userManager.CreateAsync(user, password);

        if (!result.Succeeded)
        {
            var errors = string.Join("; ", result.Errors.Select(e => e.Description));
            logger.LogError("DevSeeder: failed to create admin user '{Email}': {Errors}", email, errors);
            return;
        }

        await userManager.AddToRoleAsync(user, Permissions.AdminRole);
        logger.LogInformation("DevSeeder: created admin user '{Email}' and assigned '{Role}' role.", email, Permissions.AdminRole);
    }
}
