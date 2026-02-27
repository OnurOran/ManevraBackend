using Microsoft.EntityFrameworkCore;
using MyApp.Api.Infrastructure.Persistence;
using Quartz;

namespace MyApp.Api.Infrastructure.BackgroundJobs;

[DisallowConcurrentExecution]
public class CleanupExpiredRefreshTokensJob : IJob
{
    private readonly AppDbContext _db;
    private readonly ILogger<CleanupExpiredRefreshTokensJob> _logger;

    public CleanupExpiredRefreshTokensJob(AppDbContext db, ILogger<CleanupExpiredRefreshTokensJob> logger)
    {
        _db = db;
        _logger = logger;
    }

    public async Task Execute(IJobExecutionContext context)
    {
        var cutoff = DateTime.UtcNow;
        var deleted = await _db.RefreshTokens
            .IgnoreQueryFilters()
            .Where(t => t.ExpiresAt < cutoff || t.IsRevoked)
            .ExecuteDeleteAsync(context.CancellationToken);

        _logger.LogInformation("CleanupExpiredRefreshTokensJob: deleted {Count} expired/revoked tokens", deleted);
    }
}
