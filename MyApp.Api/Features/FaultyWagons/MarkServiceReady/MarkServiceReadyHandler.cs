using Microsoft.EntityFrameworkCore;
using MyApp.Api.Common.Behaviors;
using MyApp.Api.Common.Models;
using MyApp.Api.Domain.Entities.Manevra;
using MyApp.Api.Infrastructure.Hubs;
using MyApp.Api.Infrastructure.Persistence;

namespace MyApp.Api.Features.FaultyWagons.MarkServiceReady;

public class MarkServiceReadyHandler : ICommandHandler<MarkServiceReadyCommand, bool>
{
    private readonly AppDbContext _db;
    private readonly INotificationService _notifications;

    public MarkServiceReadyHandler(AppDbContext db, INotificationService notifications)
    {
        _db = db;
        _notifications = notifications;
    }

    public async Task<Result<bool>> Handle(MarkServiceReadyCommand command, CancellationToken ct)
    {
        var entry = await _db.FaultyWagonEntries
            .Include(e => e.Wagon)
            .FirstOrDefaultAsync(e => e.Id == command.FaultyEntryId, ct);

        if (entry is null)
            return Result<bool>.Failure("Kayıt bulunamadı.");

        if (entry.Wagon.Status != WagonStatus.CalismaYapilacak)
            return Result<bool>.Failure("Sadece kırmızı durumdaki vagonlar servise hazır yapılabilir.");

        entry.Wagon.SetStatus(WagonStatus.ServiseHazir);
        // Wagon stays in faulty list — only removed when status becomes Servis
        await _db.SaveChangesAsync(ct);
        await _notifications.SendToGroupAsync("manevra", "TrackLayoutUpdated", new { }, ct);

        return Result<bool>.Success(true);
    }
}
