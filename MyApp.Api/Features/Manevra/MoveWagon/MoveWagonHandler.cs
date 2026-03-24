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

        // ── Convoy movement ──────────────────────────────────────────────
        if (wagon.ConvoyId is not null)
            return await HandleConvoyMove(wagon, targetSlot, ct);

        // ── Single wagon movement ────────────────────────────────────────
        if (targetSlot.WagonId is not null)
            return Result<bool>.Failure("Hedef slot dolu.");

        // Block move if wagon already has a pending transfer
        var hasPendingTransfer = await _db.WagonTransfers
            .AnyAsync(t => t.WagonId == wagon.Id && !t.IsApproved, ct);
        if (hasPendingTransfer)
            return Result<bool>.Failure("Bu vagonun bekleyen bir transferi var.");

        // Block move if target slot has a pending transfer incoming
        var targetHasPending = await _db.WagonTransfers
            .AnyAsync(t => t.ToSlotId == targetSlot.Id && !t.IsApproved, ct);
        if (targetHasPending)
            return Result<bool>.Failure("Hedef slotta bekleyen bir transfer var.");

        var sourceSlot = await _db.TrackSlots
            .Include(s => s.Track)
            .FirstOrDefaultAsync(s => s.WagonId == wagon.Id, ct);

        var sourceZone = sourceSlot?.Track.Zone;
        var targetZone = targetSlot.Track.Zone;

        // ── Zone 3 → Zone 3 (internal move within Cari Hatta) ──────────
        if (sourceZone == TrackZone.CariHattaHazirDiziler && targetZone == TrackZone.CariHattaHazirDiziler)
        {
            // Check IsOnlyMiddle constraint for head/tail positions
            if (targetSlot.SlotIndex == 1 || targetSlot.SlotIndex == GetMaxSlotIndex(targetSlot))
            {
                if (wagon.IsOnlyMiddle)
                    return Result<bool>.Failure("Baş veya son slota sadece IsOnlyMiddle=FALSE olan vagonlar yerleştirilebilir.");
            }

            sourceSlot!.SetWagonId(null);
            targetSlot.SetWagonId(wagon.Id);
            await _db.SaveChangesAsync(ct);
            await _notifications.SendToGroupAsync("manevra", "TrackLayoutUpdated", new { }, ct);
            return Result<bool>.Success(true);
        }

        // ── Zone 3 target (entering Zone 3 from outside) ───────────────
        if (targetZone == TrackZone.CariHattaHazirDiziler)
        {
            var zoneError = ValidateZone3SingleWagon(wagon, sourceZone);
            if (zoneError is not null)
                return Result<bool>.Failure(zoneError);

            if (targetSlot.SlotIndex == 1 || targetSlot.SlotIndex == GetMaxSlotIndex(targetSlot))
            {
                if (wagon.IsOnlyMiddle)
                    return Result<bool>.Failure("Baş veya son slota sadece IsOnlyMiddle=FALSE olan vagonlar yerleştirilebilir.");
            }

            if (sourceSlot is not null)
                sourceSlot.SetWagonId(null);
            targetSlot.SetWagonId(wagon.Id);
            await _db.SaveChangesAsync(ct);
            await _notifications.SendToGroupAsync("manevra", "TrackLayoutUpdated", new { }, ct);
            return Result<bool>.Success(true);
        }

        // ── Zone 3 source (exiting Zone 3) — requires approval ──────────
        if (sourceZone == TrackZone.CariHattaHazirDiziler)
        {
            return await CreatePendingTransfer(wagon, sourceSlot!, targetSlot, ct);
        }

        // ── All other moves: Direct ─────────────────────────────────────
        if (sourceSlot is not null)
            sourceSlot.SetWagonId(null);
        targetSlot.SetWagonId(wagon.Id);
        await _db.SaveChangesAsync(ct);
        await _notifications.SendToGroupAsync("manevra", "TrackLayoutUpdated", new { }, ct);
        return Result<bool>.Success(true);
    }

    private async Task<Result<bool>> HandleConvoyMove(
        Wagon draggedWagon, TrackSlot targetSlot, CancellationToken ct)
    {
        // Load all convoy wagons with their current slots
        var convoyWagons = await _db.Wagons
            .Where(w => w.ConvoyId == draggedWagon.ConvoyId)
            .ToListAsync(ct);

        // Get source slots for all convoy wagons, sorted by SlotIndex to preserve order
        var convoyWagonIds = convoyWagons.Select(w => w.Id).ToHashSet();
        var sourceSlots = await _db.TrackSlots
            .Include(s => s.Track)
            .Where(s => s.WagonId != null && convoyWagonIds.Contains(s.WagonId.Value))
            .OrderBy(s => s.SlotIndex)
            .ToListAsync(ct);

        // Build ordered list: wagons sorted by their current slot position
        var orderedWagons = sourceSlots
            .Select(s => convoyWagons.First(w => w.Id == s.WagonId))
            .ToList();

        // Include any unplaced convoy wagons at the end
        var placedIds = sourceSlots.Select(s => s.WagonId!.Value).ToHashSet();
        orderedWagons.AddRange(convoyWagons.Where(w => !placedIds.Contains(w.Id)));

        var convoySize = orderedWagons.Count;
        var targetZone = targetSlot.Track.Zone;

        // Find consecutive empty slots on the same track + sectionType
        var trackSlots = await _db.TrackSlots
            .Where(s => s.TrackId == targetSlot.TrackId && s.SectionType == targetSlot.SectionType)
            .OrderBy(s => s.SlotIndex)
            .ToListAsync(ct);

        // Find N consecutive slots starting from targetSlot.SlotIndex
        // A slot is "empty" if WagonId is null OR occupied by a wagon in THIS convoy
        var startIdx = trackSlots.FindIndex(s => s.Id == targetSlot.Id);
        if (startIdx < 0)
            return Result<bool>.Failure("Target slot not found on track.");

        var consecutiveSlots = new List<TrackSlot>();
        for (var i = startIdx; i < trackSlots.Count && consecutiveSlots.Count < convoySize; i++)
        {
            var slot = trackSlots[i];
            if (slot.WagonId is null || convoyWagonIds.Contains(slot.WagonId.Value))
                consecutiveSlots.Add(slot);
            else
                break; // non-empty slot breaks the consecutive chain
        }

        if (consecutiveSlots.Count < convoySize)
            return Result<bool>.Failure("Hedef yolda yeterli ardışık boş slot bulunmuyor.");

        // ── Zone 3 convoy validation ─────────────────────────────────────
        if (targetZone == TrackZone.CariHattaHazirDiziler)
        {
            var sourceZones = sourceSlots.Select(s => s.Track.Zone).Distinct().ToList();

            foreach (var w in orderedWagons)
            {
                if (w.Line == WagonLine.Tramvay)
                    return Result<bool>.Failure("Konvoy vagonları Cari Hatta kurallarını karşılamıyor.");

                if (w.Status != WagonStatus.Servis)
                    return Result<bool>.Failure("Konvoy vagonları Cari Hatta kurallarını karşılamıyor.");
            }

            // All wagons must currently be in Zone 1 or Zone 2
            var hasUnplaced = orderedWagons.Any(w => !placedIds.Contains(w.Id));
            if (hasUnplaced || sourceZones.Any(z => z is not (TrackZone.Garaj or TrackZone.Atolye)))
                return Result<bool>.Failure("Konvoy vagonları Cari Hatta kurallarını karşılamıyor.");

            // First and last wagon: IsOnlyMiddle must be false
            var firstWagon = orderedWagons[0];
            var lastWagon = orderedWagons[^1];
            if (firstWagon.IsOnlyMiddle || lastWagon.IsOnlyMiddle)
                return Result<bool>.Failure("Konvoy vagonları Cari Hatta kurallarını karşılamıyor.");
        }

        // ── Zone 3 source (exiting Zone 3) — requires approval ──────────
        var anyFromZone3 = sourceSlots.Any(s => s.Track.Zone == TrackZone.CariHattaHazirDiziler);
        if (anyFromZone3 && targetZone != TrackZone.CariHattaHazirDiziler)
        {
            // Create pending transfers for ALL convoy wagons to consecutive target slots
            for (var i = 0; i < orderedWagons.Count; i++)
            {
                var w = orderedWagons[i];
                var src = sourceSlots.FirstOrDefault(s => s.WagonId == w.Id);
                if (src is null) continue;
                var transfer = WagonTransfer.Create(w.Id, src.Id, consecutiveSlots[i].Id);
                _db.WagonTransfers.Add(transfer);
            }
            await _db.SaveChangesAsync(ct);
            await _notifications.SendToGroupAsync("manevra", "TrackLayoutUpdated", new { }, ct);
            return Result<bool>.Success(true);
        }

        // ── Execute the move: clear sources, assign targets ─────────────
        foreach (var slot in sourceSlots)
            slot.SetWagonId(null);

        for (var i = 0; i < orderedWagons.Count; i++)
            consecutiveSlots[i].SetWagonId(orderedWagons[i].Id);

        await _db.SaveChangesAsync(ct);
        await _notifications.SendToGroupAsync("manevra", "TrackLayoutUpdated", new { }, ct);
        return Result<bool>.Success(true);
    }

    private static string? ValidateZone3SingleWagon(Wagon wagon, TrackZone? sourceZone)
    {
        if (wagon.Line == WagonLine.Tramvay)
            return "Tramvay vagonları Cari Hatta Hazır Diziler alanına giremez.";

        if (sourceZone is not (TrackZone.Garaj or TrackZone.Atolye))
            return "Vagon önce Garaj veya Atölye bölgesine alınmalıdır.";

        if (wagon.Status != WagonStatus.Servis)
            return "'Servis' statüsünde olmayan vagonlar Cari Hatta taşınamaz.";

        return null;
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
