namespace MyApp.Api.Common.Extensions;

public static class HealthCheckExtensions
{
    public static IServiceCollection AddHealthChecks(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddHealthChecks()
            .AddSqlServer(
                configuration.GetConnectionString("DefaultConnection")!,
                name: "sqlserver",
                tags: ["db", "ready"])
            .AddDiskStorageHealthCheck(setup =>
                setup.AddDrive("/", 1024), // 1 GB minimum
                name: "disk",
                tags: ["infrastructure"]);

        return services;
    }
}
