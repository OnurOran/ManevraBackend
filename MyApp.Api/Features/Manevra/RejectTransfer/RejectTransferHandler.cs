using Microsoft.EntityFrameworkCore;
using MyApp.Api.Common.Behaviors;
using MyApp.Api.Common.Models;
using MyApp.Api.Infrastructure.Hubs;
using MyApp.Api.Infrastructure.Persistence;

namespace MyApp.Api.Features.Manevra.RejectTransfer;

public class RejectTransferHandler : ICommandHandler<RejectTransferCommand, bool>
{
    private readonly AppDbContext _db;
    private readonly INotificationService _notifications;

    public RejectTransferHandler(AppDbContext db, INotificationService notifications)
    {
        _db = db;
        _notifications = notifications;
    }

    public async Task<Result<bool>> Handle(RejectTransferCommand command, CancellationToken ct)
    {
        var transfer = await _db.WagonTransfers
            .Include(t => t.Wagon)
            .FirstOrDefaultAsync(t => t.Id == command.TransferId, ct);
        if (transfer is null)
            return Result<bool>.Failure("Transfer not found.");

        if (transfer.IsApproved)
            return Result<bool>.Failure("Cannot reject an already approved transfer.");

        // If the wagon belongs to a convoy, reject all pending convoy transfers together
        var transfers = new List<Domain.Entities.Manevra.WagonTransfer> { transfer };
        if (transfer.Wagon.ConvoyId is not null)
        {
            var convoyWagonIds = await _db.Wagons
                .Where(w => w.ConvoyId == transfer.Wagon.ConvoyId)
                .Select(w => w.Id)
                .ToListAsync(ct);

            var siblingTransfers = await _db.WagonTransfers
                .Where(t => !t.IsApproved && t.Id != transfer.Id && convoyWagonIds.Contains(t.WagonId))
                .ToListAsync(ct);

            transfers.AddRange(siblingTransfers);
        }

        _db.WagonTransfers.RemoveRange(transfers);
        await _db.SaveChangesAsync(ct);
        await _notifications.SendToGroupAsync("manevra", "TrackLayoutUpdated", new { }, ct);
        return Result<bool>.Success(true);
    }
}
