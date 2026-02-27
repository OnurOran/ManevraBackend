using MyApp.Api.Common.Behaviors;
using MyApp.Api.Common.Models;
using MyApp.Api.Contracts.Manevra;
using MyApp.Api.Domain.Entities.Manevra;
using MyApp.Api.Infrastructure.Persistence;

namespace MyApp.Api.Features.Cleanup.AddCleanup;

public class AddCleanupHandler : ICommandHandler<AddCleanupCommand, CleanupEntryResponse>
{
    private readonly AppDbContext _db;

    public AddCleanupHandler(AppDbContext db) => _db = db;

    public async Task<Result<CleanupEntryResponse>> Handle(AddCleanupCommand command, CancellationToken ct)
    {
        var wagon = await _db.Wagons.FindAsync([command.WagonId], ct);
        if (wagon is null)
            return Result<CleanupEntryResponse>.Failure("Wagon not found.");

        var entry = CleanupHistory.Create(command.WagonId, command.CleanupDate);
        _db.CleanupHistories.Add(entry);
        await _db.SaveChangesAsync(ct);

        return Result<CleanupEntryResponse>.Success(new CleanupEntryResponse
        {
            WagonId = wagon.Id,
            WagonNumber = wagon.WagonNumber,
            Line = (byte)wagon.Line,
            CleanupDate = entry.CleanupDate,
        });
    }
}
