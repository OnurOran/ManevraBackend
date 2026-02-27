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
        var transfer = await _db.WagonTransfers.FindAsync([command.TransferId], ct);
        if (transfer is null)
            return Result<bool>.Failure("Transfer not found.");

        if (transfer.IsApproved)
            return Result<bool>.Failure("Cannot reject an already approved transfer.");

        _db.WagonTransfers.Remove(transfer);
        await _db.SaveChangesAsync(ct);
        await _notifications.SendToGroupAsync("manevra", "TrackLayoutUpdated", new { }, ct);
        return Result<bool>.Success(true);
    }
}
