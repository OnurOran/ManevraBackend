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

        // If wagon is in a convoy, update all wagons in the convoy
        if (wagon.ConvoyId.HasValue)
        {
            var convoyWagons = await _db.Wagons
                .Where(w => w.ConvoyId == wagon.ConvoyId)
                .ToListAsync(ct);
            foreach (var w in convoyWagons)
                w.SetStatus(status);
        }
        else
        {
            wagon.SetStatus(status);
        }

        await _db.SaveChangesAsync(ct);
        await _notifications.SendToGroupAsync("manevra", "TrackLayoutUpdated", new { }, ct);
        return Result<bool>.Success(true);
    }
}
