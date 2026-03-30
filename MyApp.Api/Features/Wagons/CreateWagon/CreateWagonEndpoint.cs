using MyApp.Api.Common.Authorization;
using MyApp.Api.Common.Behaviors;
using MyApp.Api.Common.Extensions;
using MyApp.Api.Common.Models;
using MyApp.Api.Contracts.Manevra;
using MyApp.Api.Common.Attributes;

namespace MyApp.Api.Features.Wagons.CreateWagon;

[MapToGroup("wagons")]
public static class CreateWagonEndpoint
{
    public static void Map(RouteGroupBuilder group)
    {
        group.MapPost("/", async Task<IResult> (
            CreateWagonRequest request,
            ICommandHandler<CreateWagonCommand, WagonResponse> handler,
            CancellationToken ct) =>
        {
            var command = new CreateWagonCommand
            {
                WagonNumber = request.WagonNumber,
                Line = request.Line,
                TechnicalUnit = request.TechnicalUnit,
            };
            var result = await handler.Handle(command, ct);
            return result.IsSuccess
                ? Results.Created($"/api/v1/wagons/{result.Value!.Id}", ApiResponse<WagonResponse>.Ok(result.Value!))
                : Results.BadRequest(ApiResponse<WagonResponse>.Fail(result.Error!));
        })
        .WithName("CreateWagon")
        .WithTags("Wagons")
        .RequirePermission(Permissions.Wagons.Create);
    }
}
