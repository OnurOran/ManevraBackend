using MyApp.Api.Common.Behaviors;
using MyApp.Api.Common.Models;
using MyApp.Api.Contracts.Manevra;
using MyApp.Api.Domain.Entities.Manevra;
using MyApp.Api.Infrastructure.Persistence;

namespace MyApp.Api.Features.Wagons.CreateWagon;

public class CreateWagonHandler : ICommandHandler<CreateWagonCommand, WagonResponse>
{
    private readonly AppDbContext _db;

    public CreateWagonHandler(AppDbContext db) => _db = db;

    public async Task<Result<WagonResponse>> Handle(CreateWagonCommand command, CancellationToken ct)
    {
        var line = (WagonLine)command.Line;
        if (!Enum.IsDefined(line))
            return Result<WagonResponse>.Failure("Invalid line value.");

        var wagon = Wagon.Create(command.WagonNumber, line);
        _db.Wagons.Add(wagon);
        await _db.SaveChangesAsync(ct);

        return Result<WagonResponse>.Success(new WagonResponse
        {
            Id = wagon.Id,
            WagonNumber = wagon.WagonNumber,
            Line = (byte)wagon.Line,
            IsOnlyMiddle = wagon.IsOnlyMiddle,
            Status = (byte)wagon.Status,
            ConvoyId = wagon.ConvoyId,
        });
    }
}
