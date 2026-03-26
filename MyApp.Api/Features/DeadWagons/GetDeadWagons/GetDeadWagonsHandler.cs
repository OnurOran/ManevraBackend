using Microsoft.EntityFrameworkCore;
using MyApp.Api.Common.Behaviors;
using MyApp.Api.Common.Models;
using MyApp.Api.Contracts.Manevra;
using MyApp.Api.Infrastructure.Persistence;

namespace MyApp.Api.Features.DeadWagons.GetDeadWagons;

public class GetDeadWagonsHandler : IQueryHandler<GetDeadWagonsQuery, List<DeadWagonResponse>>
{
    private readonly AppDbContext _db;

    public GetDeadWagonsHandler(AppDbContext db) => _db = db;

    public async Task<Result<List<DeadWagonResponse>>> Handle(GetDeadWagonsQuery query, CancellationToken ct)
    {
        var entries = await _db.DeadWagonEntries
            .AsNoTracking()
            .Include(e => e.Wagon)
            .OrderBy(e => e.Id)
            .Select(e => new DeadWagonResponse
            {
                Id = e.Id,
                WagonId = e.WagonId,
                WagonNumber = e.Wagon.WagonNumber,
                Line = (byte)e.Wagon.Line,
                Status = (byte)e.Wagon.Status,
                IsOnlyMiddle = e.Wagon.IsOnlyMiddle,
            })
            .ToListAsync(ct);

        return Result<List<DeadWagonResponse>>.Success(entries);
    }
}
