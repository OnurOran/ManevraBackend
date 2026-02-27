using Microsoft.EntityFrameworkCore;
using MyApp.Api.Common.Behaviors;
using MyApp.Api.Common.Models;
using MyApp.Api.Infrastructure.Hubs;
using MyApp.Api.Infrastructure.Persistence;

namespace MyApp.Api.Features.Manevra.DisbandConvoy;

public class DisbandConvoyHandler : ICommandHandler<DisbandConvoyCommand, bool>
{
    private readonly AppDbContext _db;
    private readonly INotificationService _notifications;

    public DisbandConvoyHandler(AppDbContext db, INotificationService notifications)
    {
        _db = db;
        _notifications = notifications;
    }

    public async Task<Result<bool>> Handle(DisbandConvoyCommand command, CancellationToken ct)
    {
        var convoy = await _db.Convoys.FindAsync([command.ConvoyId], ct);
        if (convoy is null)
            return Result<bool>.Failure("Convoy not found.");

        var wagons = await _db.Wagons
            .Where(w => w.ConvoyId == command.ConvoyId)
            .ToListAsync(ct);

        foreach (var wagon in wagons)
            wagon.SetConvoyId(null);

        _db.Convoys.Remove(convoy);
        await _db.SaveChangesAsync(ct);
        await _notifications.SendToGroupAsync("manevra", "TrackLayoutUpdated", new { }, ct);
        return Result<bool>.Success(true);
    }
}
