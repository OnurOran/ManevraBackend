using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using MyApp.Api.Common.Services;
using MyApp.Api.Domain.Entities;
using MyApp.Api.Domain.Entities.Manevra;

namespace MyApp.Api.Infrastructure.Persistence;

public class AppDbContext : IdentityDbContext<ApplicationUser, Microsoft.AspNetCore.Identity.IdentityRole<Guid>, Guid>
{
    private readonly ICurrentUserService _currentUserService;

    public AppDbContext(DbContextOptions<AppDbContext> options, ICurrentUserService currentUserService)
        : base(options)
    {
        _currentUserService = currentUserService;
    }

    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
    public DbSet<Permission> Permissions => Set<Permission>();
    public DbSet<RolePermission> RolePermissions => Set<RolePermission>();

    // Manevra
    public DbSet<Wagon> Wagons => Set<Wagon>();
    public DbSet<Convoy> Convoys => Set<Convoy>();
    public DbSet<Track> Tracks => Set<Track>();
    public DbSet<TrackSlot> TrackSlots => Set<TrackSlot>();
    public DbSet<WagonTransfer> WagonTransfers => Set<WagonTransfer>();
    public DbSet<CleanupHistory> CleanupHistories => Set<CleanupHistory>();
    public DbSet<WeeklyMaintenanceEntry> WeeklyMaintenanceEntries => Set<WeeklyMaintenanceEntry>();
    public DbSet<DeadWagonEntry> DeadWagonEntries => Set<DeadWagonEntry>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        builder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);

        foreach (var entityType in builder.Model.GetEntityTypes())
        {
            if (!typeof(BaseEntity).IsAssignableFrom(entityType.ClrType)) continue;
            var method = typeof(AppDbContext)
                .GetMethod(nameof(SetSoftDeleteFilter), System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static)!
                .MakeGenericMethod(entityType.ClrType);
            method.Invoke(null, [builder]);
        }
    }

    private static void SetSoftDeleteFilter<T>(ModelBuilder builder) where T : BaseEntity
    {
        builder.Entity<T>().HasQueryFilter(e => !e.IsDeleted);
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var userId = _currentUserService.UserId;

        foreach (var entry in ChangeTracker.Entries<BaseEntity>())
        {
            switch (entry.State)
            {
                case EntityState.Added:
                    entry.Entity.SetCreated(userId);
                    break;
                case EntityState.Modified:
                    entry.Entity.SetUpdated(userId);
                    break;
            }
        }

        return await base.SaveChangesAsync(cancellationToken);
    }
}
