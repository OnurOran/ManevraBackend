using Microsoft.EntityFrameworkCore;
using MyApp.Api.Common.Behaviors;
using MyApp.Api.Common.Models;
using MyApp.Api.Domain.Entities.Manevra;
using MyApp.Api.Infrastructure.Hubs;
using MyApp.Api.Infrastructure.Persistence;

namespace MyApp.Api.Features.Wagons.UpdateWagonStatus;

public class UpdateWagonStatusHandler : ICommandHandler<UpdateWagonStatusCommand, bool>
{
    private readonly AppDbContext _db;
    private readonly INotificationService _notifications;

    public UpdateWagonStatusHandler(AppDbContext db, INotificationService notifications)
    {
        _db = db;
        _notifications = notifications;
    }

    public async Task<Result<bool>> Handle(UpdateWagonStatusCommand command, CancellationToken ct)
    {
        var status = (WagonStatus)command.Status;
        if (!Enum.IsDefined(status))
            return Result<bool>.Failure("Invalid status value.");

        var wagon = await _db.Wagons.FindAsync([command.WagonId], ct);
        if (wagon is null)
            return Result<bool>.Failure("Wagon not found.");

        // Collect all wagons to update (convoy or single)
        List<Wagon> wagonsToUpdate;
        if (wagon.ConvoyId.HasValue)
        {
            wagonsToUpdate = await _db.Wagons
                .Where(w => w.ConvoyId == wagon.ConvoyId)
                .ToListAsync(ct);
        }
        else
        {
            wagonsToUpdate = [wagon];
        }

        foreach (var w in wagonsToUpdate)
            w.SetStatus(status);

        // Faulty wagon tracking
        foreach (var w in wagonsToUpdate)
            await SyncFaultyEntry(w, status, ct);

        await _db.SaveChangesAsync(ct);
        await _notifications.SendToGroupAsync("manevra", "TrackLayoutUpdated", new { }, ct);
        return Result<bool>.Success(true);
    }

    private async Task SyncFaultyEntry(Wagon wagon, WagonStatus newStatus, CancellationToken ct)
    {
        var existing = await _db.FaultyWagonEntries
            .FirstOrDefaultAsync(e => e.WagonId == wagon.Id, ct);

        if (newStatus == WagonStatus.CalismaYapilacak && existing is null)
        {
            // Became red → add to faulty list
            _db.FaultyWagonEntries.Add(FaultyWagonEntry.Create(wagon.Id));
        }
        else if (newStatus == WagonStatus.Servis && existing is not null)
        {
            // Became green → remove from faulty list
            _db.FaultyWagonEntries.Remove(existing);
        }
        // ServiseHazir (yellow) → no change, stays in faulty list if already there
    }
}
