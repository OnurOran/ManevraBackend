using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Testcontainers.MsSql;

namespace MyApp.Api.Tests;

/// <summary>
/// Spins up the full ASP.NET Core application against a real SQL Server container.
/// Shared via ICollectionFixture so all test classes in the "Api" collection reuse
/// one container, keeping the test suite fast without sacrificing realism.
///
/// On startup the app automatically:
///   1. Runs EF Core migrations (Program.cs Development block)
///   2. Runs DevSeeder — creates the Admin role and an admin user with the
///      credentials defined by AdminEmail / AdminPassword below.
/// </summary>
public class ApiWebApplicationFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly MsSqlContainer _sqlServer = new MsSqlBuilder("mcr.microsoft.com/mssql/server:2022-latest")
        .Build();

    /// <summary>Seeded admin credentials. Use these in tests that require login.</summary>
    public const string AdminEmail = "admin@test.example";
    public const string AdminPassword = "Admin123!";

    public async Task InitializeAsync() => await _sqlServer.StartAsync();

    public new async Task DisposeAsync() => await _sqlServer.DisposeAsync();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        // Development environment triggers auto-migration + DevSeeder in Program.cs.
        builder.UseEnvironment("Development");

        builder.ConfigureAppConfiguration((_, config) =>
        {
            config.AddInMemoryCollection(new Dictionary<string, string?>
            {
                // Route the app to the testcontainer database.
                ["ConnectionStrings:DefaultConnection"] = _sqlServer.GetConnectionString(),

                // Predictable admin credentials so tests can log in without guessing.
                ["Seed:AdminEmail"] = AdminEmail,
                ["Seed:AdminPassword"] = AdminPassword,

                // Raise rate-limit ceilings so no test can get a 429.
                // RateLimitingExtensions reads these values with fallback to production defaults.
                ["RateLimit:Default:PermitLimit"] = "10000",
                ["RateLimit:Auth:PermitLimit"] = "10000",
            });
        });
    }
}

[CollectionDefinition("Api")]
public class ApiCollection : ICollectionFixture<ApiWebApplicationFactory> { }
