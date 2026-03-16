using MyApp.Api.Common.Behaviors;
using MyApp.Api.Common.Models;
using MyApp.Api.Infrastructure.Hubs;
using MyApp.Api.Infrastructure.Persistence;

namespace MyApp.Api.Features.WeeklyMaintenance.RemoveMaintenanceEntry;

public class RemoveMaintenanceEntryHandler : ICommandHandler<RemoveMaintenanceEntryCommand, bool>
{
    private readonly AppDbContext _db;
    private readonly INotificationService _notifications;

    public RemoveMaintenanceEntryHandler(AppDbContext db, INotificationService notifications)
    {
        _db = db;
        _notifications = notifications;
    }

    public async Task<Result<bool>> Handle(RemoveMaintenanceEntryCommand command, CancellationToken ct)
    {
        var entry = await _db.WeeklyMaintenanceEntries.FindAsync([command.EntryId], ct);
        if (entry is null)
            return Result<bool>.Failure("Entry not found.");

        _db.WeeklyMaintenanceEntries.Remove(entry);
        await _db.SaveChangesAsync(ct);
        await _notifications.SendToGroupAsync("weekly-maintenance", "WeeklyMaintenanceUpdated", new { }, ct);

        return Result<bool>.Success(true);
    }
}
