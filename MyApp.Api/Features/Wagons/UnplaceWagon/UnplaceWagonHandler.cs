using Microsoft.EntityFrameworkCore;
using MyApp.Api.Common.Behaviors;
using MyApp.Api.Common.Models;
using MyApp.Api.Infrastructure.Hubs;
using MyApp.Api.Infrastructure.Persistence;

namespace MyApp.Api.Features.Wagons.UnplaceWagon;

public class UnplaceWagonHandler : ICommandHandler<UnplaceWagonCommand, bool>
{
    private readonly AppDbContext _db;
    private readonly INotificationService _notifications;

    public UnplaceWagonHandler(AppDbContext db, INotificationService notifications)
    {
        _db = db;
        _notifications = notifications;
    }

    public async Task<Result<bool>> Handle(UnplaceWagonCommand command, CancellationToken ct)
    {
        var wagon = await _db.Wagons.FindAsync([command.WagonId], ct);
        if (wagon is null)
            return Result<bool>.Failure("Vagon bulunamadı.");

        if (wagon.ConvoyId is not null)
            return Result<bool>.Failure("Dizideki vagonlar yerinden kaldırılamaz. Önce diziyi bozun.");

        var hasPending = await _db.WagonTransfers
            .AnyAsync(t => t.WagonId == wagon.Id && !t.IsApproved, ct);
        if (hasPending)
            return Result<bool>.Failure("Bekleyen transferi olan vagonlar yerinden kaldırılamaz.");

        var slot = await _db.TrackSlots
            .FirstOrDefaultAsync(s => s.WagonId == wagon.Id, ct);

        if (slot is null)
            return Result<bool>.Failure("Vagon zaten yerleştirilmemiş.");

        slot.SetWagonId(null);
        await _db.SaveChangesAsync(ct);
        await _notifications.SendToGroupAsync("manevra", "TrackLayoutUpdated", new { }, ct);

        return Result<bool>.Success(true);
    }
}
