using Amazon.S3;
using Amazon.Runtime;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using MyApp.Api.Common.Authorization;
using MyApp.Api.Common.Exceptions;
using MyApp.Api.Common.Services;
using MyApp.Api.Infrastructure.BackgroundJobs;
using MyApp.Api.Infrastructure.Email;
using MyApp.Api.Infrastructure.Hubs;
using MyApp.Api.Infrastructure.Persistence;
using MyApp.Api.Infrastructure.Storage;
using Quartz;

namespace MyApp.Api.Common.Extensions;

public static class InfrastructureExtensions
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        // Database
        services.AddDbContext<AppDbContext>(options =>
            options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));

        // Current user
        services.AddHttpContextAccessor();
        services.AddScoped<ICurrentUserService, CurrentUserService>();

        // Options with startup validation
        services.AddOptions<JwtOptions>()
            .Bind(configuration.GetSection(JwtOptions.SectionName))
            .Validate(o => !string.IsNullOrWhiteSpace(o.Secret)
                        && o.Secret.Length >= 32
                        && o.Secret != "CHANGE_ME_TO_A_LONG_SECRET_KEY_AT_LEAST_32_CHARS",
                "Jwt:Secret must be at least 32 characters and must not be the default placeholder value.")
            .ValidateOnStart();

        services.AddOptions<StorageOptions>()
            .Bind(configuration.GetSection(StorageOptions.SectionName))
            .Validate(o => !string.IsNullOrWhiteSpace(o.BucketName),
                "Storage:BucketName is required.")
            .ValidateOnStart();

        services.AddOptions<EmailOptions>()
            .Bind(configuration.GetSection(EmailOptions.SectionName))
            .Validate(o => !string.IsNullOrWhiteSpace(o.Host),
                "Email:Host is required.")
            .ValidateOnStart();

        // JWT token service
        services.AddScoped<JwtTokenService>();

        // Storage
        services.AddSingleton<IAmazonS3>(sp =>
        {
            var opts = sp.GetRequiredService<IOptions<StorageOptions>>().Value;
            var config = new AmazonS3Config
            {
                ServiceURL = opts.ServiceUrl,
                ForcePathStyle = true,
                Timeout = TimeSpan.FromSeconds(30),
                MaxErrorRetry = 3
            };
            var credentials = new BasicAWSCredentials(opts.AccessKey, opts.SecretKey);
            return new AmazonS3Client(credentials, config);
        });
        services.AddScoped<IFileService, HetznerFileService>();

        // Email
        services.AddScoped<IEmailService, MailKitEmailService>();

        // Exception handler
        services.AddExceptionHandler<GlobalExceptionHandler>();
        services.AddProblemDetails();

        // Validators
        services.AddValidatorsFromAssemblyContaining<Program>(ServiceLifetime.Scoped);

        // SignalR
        services.AddSignalR();
        services.AddScoped<INotificationService, NotificationService>();

        // Permission sync + Admin role seed (runs on startup)
        services.AddHostedService<PermissionSyncService>();

        // Background jobs
        services.AddQuartz(q =>
        {
            var cleanupKey = new JobKey(nameof(CleanupExpiredRefreshTokensJob));
            q.AddJob<CleanupExpiredRefreshTokensJob>(opts => opts.WithIdentity(cleanupKey));
            q.AddTrigger(opts => opts
                .ForJob(cleanupKey)
                .WithIdentity($"{nameof(CleanupExpiredRefreshTokensJob)}-trigger")
                .WithCronSchedule(
                    configuration["Quartz:CleanupExpiredRefreshTokensCron"] ?? "0 0 3 * * ?"));
        });
        services.AddQuartzHostedService(q => q.WaitForJobsToComplete = true);

        return services;
    }
}
