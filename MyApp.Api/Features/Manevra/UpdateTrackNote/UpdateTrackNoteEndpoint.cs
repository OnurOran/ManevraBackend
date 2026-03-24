using MyApp.Api.Common.Authorization;
using MyApp.Api.Common.Behaviors;
using MyApp.Api.Common.Extensions;
using MyApp.Api.Common.Models;
using MyApp.Api.Contracts.Manevra;
using MyApp.Api.Common.Attributes;

namespace MyApp.Api.Features.Manevra.UpdateTrackNote;

[MapToGroup("manevra")]
public static class UpdateTrackNoteEndpoint
{
    public static void Map(RouteGroupBuilder group)
    {
        group.MapPut("/tracks/{id:int}/note", async Task<IResult> (
            int id,
            UpdateTrackNoteRequest request,
            ICommandHandler<UpdateTrackNoteCommand, bool> handler,
            CancellationToken ct) =>
        {
            var command = new UpdateTrackNoteCommand
            {
                TrackId = id,
                Note = request.Note,
            };
            var result = await handler.Handle(command, ct);
            return result.IsSuccess
                ? Results.Ok(ApiResponse.Ok())
                : Results.BadRequest(ApiResponse.Fail(result.Error!));
        })
        .WithName("UpdateTrackNote")
        .WithTags("Manevra")
        .RequirePermission(Permissions.Manevra.Edit);
    }
}
