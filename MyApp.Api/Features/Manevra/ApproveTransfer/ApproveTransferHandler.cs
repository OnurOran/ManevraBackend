using Microsoft.EntityFrameworkCore;
using MyApp.Api.Common.Behaviors;
using MyApp.Api.Common.Models;
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
            .Include(t => t.FromSlot)
            .Include(t => t.ToSlot)
            .FirstOrDefaultAsync(t => t.Id == command.TransferId, ct);

        if (transfer is null)
            return Result<bool>.Failure("Transfer not found.");

        if (transfer.IsApproved)
            return Result<bool>.Failure("Transfer already approved.");

        // Verify target slot is still empty
        if (transfer.ToSlot.WagonId is not null)
            return Result<bool>.Failure("Target slot is no longer empty.");

        // Execute the actual move
        transfer.FromSlot.SetWagonId(null);
        transfer.ToSlot.SetWagonId(transfer.WagonId);
        transfer.Approve();

        await _db.SaveChangesAsync(ct);
        await _notifications.SendToGroupAsync("manevra", "TrackLayoutUpdated", new { }, ct);
        return Result<bool>.Success(true);
    }
}
