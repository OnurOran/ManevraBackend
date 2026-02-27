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
            return Result<Guid>.Failure("A convoy requires at least 2 wagons.");

        var wagons = await _db.Wagons
            .Where(w => command.WagonIds.Contains(w.Id))
            .ToListAsync(ct);

        if (wagons.Count != command.WagonIds.Count)
            return Result<Guid>.Failure("One or more wagons not found.");

        if (wagons.Any(w => w.ConvoyId.HasValue))
            return Result<Guid>.Failure("One or more wagons are already in a convoy.");

        var convoy = Convoy.Create();
        _db.Convoys.Add(convoy);

        foreach (var wagon in wagons)
            wagon.SetConvoyId(convoy.Id);

        await _db.SaveChangesAsync(ct);
        await _notifications.SendToGroupAsync("manevra", "TrackLayoutUpdated", new { }, ct);
        return Result<Guid>.Success(convoy.Id);
    }
}
