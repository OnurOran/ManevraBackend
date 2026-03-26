using Microsoft.EntityFrameworkCore;
using MyApp.Api.Common.Behaviors;
using MyApp.Api.Common.Models;
using MyApp.Api.Contracts.Manevra;
using MyApp.Api.Domain.Entities.Manevra;
using MyApp.Api.Infrastructure.Hubs;
using MyApp.Api.Infrastructure.Persistence;

namespace MyApp.Api.Features.DeadWagons.AddDeadWagon;

public class AddDeadWagonHandler : ICommandHandler<AddDeadWagonCommand, DeadWagonResponse>
{
    private readonly AppDbContext _db;
    private readonly INotificationService _notifications;

    public AddDeadWagonHandler(AppDbContext db, INotificationService notifications)
    {
        _db = db;
        _notifications = notifications;
    }

    public async Task<Result<DeadWagonResponse>> Handle(AddDeadWagonCommand command, CancellationToken ct)
    {
        var wagon = await _db.Wagons.FindAsync([command.WagonId], ct);
        if (wagon is null)
            return Result<DeadWagonResponse>.Failure("Vagon bulunamadı.");

        // Block if wagon is in a convoy
        if (wagon.ConvoyId is not null)
            return Result<DeadWagonResponse>.Failure("Dizideki vagonlar servis dışı yapılamaz.");

        // Check if already in dead list
        var exists = await _db.DeadWagonEntries.AnyAsync(e => e.WagonId == command.WagonId, ct);
        if (exists)
            return Result<DeadWagonResponse>.Failure("Vagon zaten servis dışı listesinde.");

        // Max 4 entries
        var count = await _db.DeadWagonEntries.CountAsync(ct);
        if (count >= 4)
            return Result<DeadWagonResponse>.Failure("Servis dışı listesi dolu (maks 4).");

        var entry = DeadWagonEntry.Create(command.WagonId);
        _db.DeadWagonEntries.Add(entry);
        await _db.SaveChangesAsync(ct);
        await _notifications.SendToGroupAsync("manevra", "TrackLayoutUpdated", new { }, ct);

        return Result<DeadWagonResponse>.Success(new DeadWagonResponse
        {
            Id = entry.Id,
            WagonId = wagon.Id,
            WagonNumber = wagon.WagonNumber,
            Line = (byte)wagon.Line,
            Status = (byte)wagon.Status,
            IsOnlyMiddle = wagon.IsOnlyMiddle,
        });
    }
}
