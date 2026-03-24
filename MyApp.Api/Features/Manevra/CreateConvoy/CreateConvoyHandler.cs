using Microsoft.EntityFrameworkCore;
using MyApp.Api.Common.Behaviors;
using MyApp.Api.Common.Models;
using MyApp.Api.Domain.Entities.Manevra;
using MyApp.Api.Infrastructure.Hubs;
using MyApp.Api.Infrastructure.Persistence;

namespace MyApp.Api.Features.Manevra.CreateConvoy;

public class CreateConvoyHandler : ICommandHandler<CreateConvoyCommand, Guid>
{
    private readonly AppDbContext _db;
    private readonly INotificationService _notifications;

    public CreateConvoyHandler(AppDbContext db, INotificationService notifications)
    {
        _db = db;
        _notifications = notifications;
    }

    public async Task<Result<Guid>> Handle(CreateConvoyCommand command, CancellationToken ct)
    {
        if (command.WagonIds.Count < 2)
            return Result<Guid>.Failure("Dizi için en az 2 vagon gereklidir.");

        var wagons = await _db.Wagons
            .Where(w => command.WagonIds.Contains(w.Id))
            .ToListAsync(ct);

        if (wagons.Count != command.WagonIds.Count)
            return Result<Guid>.Failure("Bir veya daha fazla vagon bulunamadı.");

        // If some wagons are already in a convoy, they must all share the same one (expand scenario)
        var existingConvoyIds = wagons.Where(w => w.ConvoyId.HasValue).Select(w => w.ConvoyId!.Value).Distinct().ToList();
        if (existingConvoyIds.Count > 1)
            return Result<Guid>.Failure("Vagonlar farklı dizilere ait.");

        var existingConvoyId = existingConvoyIds.FirstOrDefault();

        // Validate: all wagons must be on the same track and in adjacent slots
        var slots = await _db.TrackSlots
            .Where(s => s.WagonId.HasValue && command.WagonIds.Contains(s.WagonId.Value))
            .ToListAsync(ct);

        if (slots.Count != wagons.Count)
            return Result<Guid>.Failure("Tüm vagonlar bir yola yerleştirilmiş olmalıdır.");

        var trackIds = slots.Select(s => s.TrackId).Distinct().ToList();
        if (trackIds.Count != 1)
            return Result<Guid>.Failure("Tüm vagonlar aynı yolda olmalıdır.");

        var sectionTypes = slots.Select(s => s.SectionType).Distinct().ToList();
        if (sectionTypes.Count != 1)
            return Result<Guid>.Failure("Tüm vagonlar aynı bölümde olmalıdır.");

        // Check adjacency: slot indexes must form a consecutive sequence
        var sortedIndexes = slots.OrderBy(s => s.SlotIndex).Select(s => (int)s.SlotIndex).ToList();
        for (var i = 1; i < sortedIndexes.Count; i++)
        {
            if (sortedIndexes[i] != sortedIndexes[i - 1] + 1)
                return Result<Guid>.Failure("Vagonlar yan yana slotlarda olmalıdır.");
        }

        Convoy convoy;
        if (existingConvoyId != Guid.Empty)
        {
            // Expand existing convoy — reuse it, just assign new members
            convoy = (await _db.Convoys.FindAsync([existingConvoyId], ct))!;
        }
        else
        {
            convoy = Convoy.Create();
            _db.Convoys.Add(convoy);
        }

        foreach (var wagon in wagons.Where(w => w.ConvoyId != convoy.Id))
            wagon.SetConvoyId(convoy.Id);

        await _db.SaveChangesAsync(ct);
        await _notifications.SendToGroupAsync("manevra", "TrackLayoutUpdated", new { }, ct);
        return Result<Guid>.Success(convoy.Id);
    }
}
