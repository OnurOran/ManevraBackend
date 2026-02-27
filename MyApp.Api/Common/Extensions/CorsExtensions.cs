namespace MyApp.Api.Common.Extensions;

public static class CorsExtensions
{
    public const string PolicyName = "AppCorsPolicy";

    public static IServiceCollection AddCorsPolicy(this IServiceCollection services, IConfiguration configuration)
    {
        var allowedOrigins = configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? [];

        if (allowedOrigins.Length == 0)
            throw new InvalidOperationException(
                "Cors:AllowedOrigins must contain at least one origin. " +
                "Configure it in appsettings.json or via environment variables.");

        services.AddCors(options =>
        {
            options.AddPolicy(PolicyName, policy =>
            {
                policy.WithOrigins(allowedOrigins)
                      .AllowAnyHeader()
                      .AllowAnyMethod()
                      .AllowCredentials(); // required for SignalR
            });
        });

        return services;
    }
}
