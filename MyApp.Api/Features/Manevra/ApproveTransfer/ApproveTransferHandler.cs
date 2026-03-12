using Microsoft.EntityFrameworkCore;
using MyApp.Api.Common.Behaviors;
using MyApp.Api.Common.Models;
using MyApp.Api.Domain.Entities.Manevra;
using MyApp.Api.Infrastructure.Hubs;
using MyApp.Api.Infrastructure.Persistence;

namespace MyApp.Api.Features.Manevra.ApproveTransfer;

public class ApproveTransferHandler : ICommandHandler<ApproveTransferCommand, bool>
{
    private readonly AppDbContext _db;
    private readonly INotificationService _notifications;

    public ApproveTransferHandler(AppDbContext db, INotificationService notifications)
    {
        _db = db;
        _notifications = notifications;
    }

    public async Task<Result<bool>> Handle(ApproveTransferCommand command, CancellationToken ct)
    {
        var transfer = await _db.WagonTransfers
            .Include(t => t.Wagon)
            .Include(t => t.FromSlot)
            .Include(t => t.ToSlot)
            .FirstOrDefaultAsync(t => t.Id == command.TransferId, ct);

        if (transfer is null)
            return Result<bool>.Failure("Transfer not found.");

        if (transfer.IsApproved)
            return Result<bool>.Failure("Transfer already approved.");

        // If the wagon belongs to a convoy, approve all pending convoy transfers together
        var transfers = new List<WagonTransfer> { transfer };
        if (transfer.Wagon.ConvoyId is not null)
        {
            var convoyWagonIds = await _db.Wagons
                .Where(w => w.ConvoyId == transfer.Wagon.ConvoyId)
                .Select(w => w.Id)
                .ToListAsync(ct);

            var siblingTransfers = await _db.WagonTransfers
                .Include(t => t.FromSlot)
                .Include(t => t.ToSlot)
                .Where(t => !t.IsApproved && t.Id != transfer.Id && convoyWagonIds.Contains(t.WagonId))
                .ToListAsync(ct);

            transfers.AddRange(siblingTransfers);
        }

        // Verify all target slots are still empty
        foreach (var t in transfers)
        {
            if (t.ToSlot.WagonId is not null)
                return Result<bool>.Failure("Target slot is no longer empty.");
        }

        // Execute moves for all transfers
        foreach (var t in transfers)
        {
            t.FromSlot.SetWagonId(null);
            t.ToSlot.SetWagonId(t.WagonId);
            t.Approve();
        }

        await _db.SaveChangesAsync(ct);
        await _notifications.SendToGroupAsync("manevra", "TrackLayoutUpdated", new { }, ct);
        return Result<bool>.Success(true);
    }
}
