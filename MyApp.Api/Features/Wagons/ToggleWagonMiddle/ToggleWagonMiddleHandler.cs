using MyApp.Api.Common.Behaviors;
using MyApp.Api.Common.Models;
using MyApp.Api.Infrastructure.Hubs;
using MyApp.Api.Infrastructure.Persistence;

namespace MyApp.Api.Features.Wagons.ToggleWagonMiddle;

public class ToggleWagonMiddleHandler : ICommandHandler<ToggleWagonMiddleCommand, bool>
{
    private readonly AppDbContext _db;
    private readonly INotificationService _notifications;

    public ToggleWagonMiddleHandler(AppDbContext db, INotificationService notifications)
    {
        _db = db;
        _notifications = notifications;
    }

    public async Task<Result<bool>> Handle(ToggleWagonMiddleCommand command, CancellationToken ct)
    {
        var wagon = await _db.Wagons.FindAsync([command.WagonId], ct);
        if (wagon is null)
            return Result<bool>.Failure("Wagon not found.");

        wagon.ToggleIsOnlyMiddle();
        await _db.SaveChangesAsync(ct);
        await _notifications.SendToGroupAsync("manevra", "TrackLayoutUpdated", new { }, ct);
        return Result<bool>.Success(true);
    }
}
