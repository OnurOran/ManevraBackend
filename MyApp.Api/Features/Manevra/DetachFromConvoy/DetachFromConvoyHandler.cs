using Microsoft.EntityFrameworkCore;
using MyApp.Api.Common.Behaviors;
using MyApp.Api.Common.Models;
using MyApp.Api.Infrastructure.Hubs;
using MyApp.Api.Infrastructure.Persistence;

namespace MyApp.Api.Features.Manevra.DetachFromConvoy;

public class DetachFromConvoyHandler : ICommandHandler<DetachFromConvoyCommand, bool>
{
    private readonly AppDbContext _db;
    private readonly INotificationService _notifications;

    public DetachFromConvoyHandler(AppDbContext db, INotificationService notifications)
    {
        _db = db;
        _notifications = notifications;
    }

    public async Task<Result<bool>> Handle(DetachFromConvoyCommand command, CancellationToken ct)
    {
        var wagon = await _db.Wagons.FindAsync([command.WagonId], ct);
        if (wagon is null)
            return Result<bool>.Failure("Wagon not found.");

        if (!wagon.ConvoyId.HasValue)
            return Result<bool>.Failure("Wagon is not in a convoy.");

        var convoyId = wagon.ConvoyId.Value;
        wagon.SetConvoyId(null);

        // If only 1 or 0 wagons remain, disband the convoy
        var remaining = await _db.Wagons.CountAsync(w => w.ConvoyId == convoyId, ct);
        if (remaining <= 1)
        {
            var lastWagon = await _db.Wagons.FirstOrDefaultAsync(w => w.ConvoyId == convoyId, ct);
            lastWagon?.SetConvoyId(null);

            var convoy = await _db.Convoys.FindAsync([convoyId], ct);
            if (convoy is not null)
                _db.Convoys.Remove(convoy);
        }

        await _db.SaveChangesAsync(ct);
        await _notifications.SendToGroupAsync("manevra", "TrackLayoutUpdated", new { }, ct);
        return Result<bool>.Success(true);
    }
}
