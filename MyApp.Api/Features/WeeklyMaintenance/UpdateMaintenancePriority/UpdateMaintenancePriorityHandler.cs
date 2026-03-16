using MyApp.Api.Common.Behaviors;
using MyApp.Api.Common.Models;
using MyApp.Api.Infrastructure.Hubs;
using MyApp.Api.Infrastructure.Persistence;

namespace MyApp.Api.Features.WeeklyMaintenance.UpdateMaintenancePriority;

public class UpdateMaintenancePriorityHandler : ICommandHandler<UpdateMaintenancePriorityCommand, bool>
{
    private readonly AppDbContext _db;
    private readonly INotificationService _notifications;

    public UpdateMaintenancePriorityHandler(AppDbContext db, INotificationService notifications)
    {
        _db = db;
        _notifications = notifications;
    }

    public async Task<Result<bool>> Handle(UpdateMaintenancePriorityCommand command, CancellationToken ct)
    {
        var entry = await _db.WeeklyMaintenanceEntries.FindAsync([command.EntryId], ct);
        if (entry is null)
            return Result<bool>.Failure("Entry not found.");

        if (command.Priority is not null and not 1 and not 2 and not 3)
            return Result<bool>.Failure("Priority must be 1, 2, or 3.");

        entry.SetPriority(command.Priority);
        await _db.SaveChangesAsync(ct);
        await _notifications.SendToGroupAsync("weekly-maintenance", "WeeklyMaintenanceUpdated", new { }, ct);

        return Result<bool>.Success(true);
    }
}
