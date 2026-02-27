using Microsoft.EntityFrameworkCore;
using MyApp.Api.Common.Behaviors;
using MyApp.Api.Common.Models;
using MyApp.Api.Domain.Entities.Manevra;
using MyApp.Api.Infrastructure.Hubs;
using MyApp.Api.Infrastructure.Persistence;

namespace MyApp.Api.Features.Manevra.MoveWagon;

public class MoveWagonHandler : ICommandHandler<MoveWagonCommand, bool>
{
    private readonly AppDbContext _db;
    private readonly INotificationService _notifications;

    public MoveWagonHandler(AppDbContext db, INotificationService notifications)
    {
        _db = db;
        _notifications = notifications;
    }

    public async Task<Result<bool>> Handle(MoveWagonCommand command, CancellationToken ct)
    {
        var wagon = await _db.Wagons.FindAsync([command.WagonId], ct);
        if (wagon is null)
            return Result<bool>.Failure("Wagon not found.");

        var targetSlot = await _db.TrackSlots
            .Include(s => s.Track)
            .FirstOrDefaultAsync(s => s.Id == command.TargetSlotId, ct);
        if (targetSlot is null)
            return Result<bool>.Failure("Target slot not found.");

        if (targetSlot.WagonId is not null)
            return Result<bool>.Failure("Target slot is not empty.");

        // Find wagon's current slot
        var sourceSlot = await _db.TrackSlots
            .Include(s => s.Track)
            .FirstOrDefaultAsync(s => s.WagonId == wagon.Id, ct);

        var sourceZone = sourceSlot?.Track.Zone;
        var targetZone = targetSlot.Track.Zone;

        // ── Zone 3 target constraints (direct move, no approval needed) ──
        if (targetZone == TrackZone.CariHattaHazirDiziler)
        {
            if (wagon.Line == WagonLine.Tramvay)
                return Result<bool>.Failure("Tramvay vagonları Cari Hatta Hazır Diziler alanına giremez.");

            if (sourceZone is not (TrackZone.Garaj or TrackZone.Atolye))
                return Result<bool>.Failure("Vagon önce Garaj veya Atölye bölgesine alınmalıdır.");

            if (wagon.Status == WagonStatus.CalismaYapilacak)
                return Result<bool>.Failure("'Çalışma Yapılacak' statüsündeki vagonlar Cari Hatta taşınamaz.");

            // IsOnlyMiddle constraint for first/last slot
            if (targetSlot.SlotIndex == 1 || targetSlot.SlotIndex == GetMaxSlotIndex(targetSlot))
            {
                if (wagon.IsOnlyMiddle)
                    return Result<bool>.Failure("Baş veya son slota sadece IsOnlyMiddle=FALSE olan vagonlar yerleştirilebilir.");
            }

            // Direct move into Zone 3 — no approval needed
            if (sourceSlot is not null)
                sourceSlot.SetWagonId(null);
            targetSlot.SetWagonId(wagon.Id);
            await _db.SaveChangesAsync(ct);
            await _notifications.SendToGroupAsync("manevra", "TrackLayoutUpdated", new { }, ct);
            return Result<bool>.Success(true);
        }

        // ── Zone 3 source (exiting Zone 3) — requires approval ─────────
        if (sourceZone == TrackZone.CariHattaHazirDiziler)
        {
            return await CreatePendingTransfer(wagon, sourceSlot!, targetSlot, ct);
        }

        // ── All other moves: Direct ────────────────────────────────────
        if (sourceSlot is not null)
            sourceSlot.SetWagonId(null);
        targetSlot.SetWagonId(wagon.Id);
        await _db.SaveChangesAsync(ct);
        await _notifications.SendToGroupAsync("manevra", "TrackLayoutUpdated", new { }, ct);
        return Result<bool>.Success(true);
    }

    private async Task<Result<bool>> CreatePendingTransfer(
        Wagon wagon, TrackSlot sourceSlot, TrackSlot targetSlot, CancellationToken ct)
    {
        var transfer = WagonTransfer.Create(wagon.Id, sourceSlot.Id, targetSlot.Id);
        _db.WagonTransfers.Add(transfer);
        await _db.SaveChangesAsync(ct);
        await _notifications.SendToGroupAsync("manevra", "TrackLayoutUpdated", new { }, ct);
        return Result<bool>.Success(true);
    }

    private byte GetMaxSlotIndex(TrackSlot slot)
    {
        // For Zone 3, each track has 4 slots
        return 4;
    }
}
