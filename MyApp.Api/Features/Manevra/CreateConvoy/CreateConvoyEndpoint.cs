using MyApp.Api.Common.Authorization;
using MyApp.Api.Common.Behaviors;
using MyApp.Api.Common.Extensions;
using MyApp.Api.Common.Models;
using MyApp.Api.Contracts.Manevra;
using MyApp.Api.Common.Attributes;

namespace MyApp.Api.Features.Manevra.CreateConvoy;

[MapToGroup("manevra")]
public static class CreateConvoyEndpoint
{
    public static void Map(RouteGroupBuilder group)
    {
        group.MapPost("/convoy", async Task<IResult> (
            CreateConvoyRequest request,
            ICommandHandler<CreateConvoyCommand, Guid> handler,
            CancellationToken ct) =>
        {
            var command = new CreateConvoyCommand { WagonIds = request.WagonIds };
            var result = await handler.Handle(command, ct);
            return result.IsSuccess
                ? Results.Ok(ApiResponse<Guid>.Ok(result.Value!))
                : Results.BadRequest(ApiResponse<Guid>.Fail(result.Error!));
        })
        .WithName("CreateConvoy")
        .WithTags("Manevra")
        .RequirePermission(Permissions.Manevra.Edit);
    }
}
