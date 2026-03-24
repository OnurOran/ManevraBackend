using MyApp.Api.Common.Behaviors;
using MyApp.Api.Common.Models;
using MyApp.Api.Infrastructure.Hubs;
using MyApp.Api.Infrastructure.Persistence;

namespace MyApp.Api.Features.DeadWagons.RemoveDeadWagon;

public class RemoveDeadWagonHandler : ICommandHandler<RemoveDeadWagonCommand, bool>
{
    private readonly AppDbContext _db;
    private readonly INotificationService _notifications;

    public RemoveDeadWagonHandler(AppDbContext db, INotificationService notifications)
    {
        _db = db;
        _notifications = notifications;
    }

    public async Task<Result<bool>> Handle(RemoveDeadWagonCommand command, CancellationToken ct)
    {
        var entry = await _db.DeadWagonEntries.FindAsync([command.EntryId], ct);
        if (entry is null)
            return Result<bool>.Failure("Kayıt bulunamadı.");

        _db.DeadWagonEntries.Remove(entry);
        await _db.SaveChangesAsync(ct);
        await _notifications.SendToGroupAsync("manevra", "TrackLayoutUpdated", new { }, ct);

        return Result<bool>.Success(true);
    }
}
