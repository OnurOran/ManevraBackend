using MyApp.Api.Common.Behaviors;

namespace MyApp.Api.Features.Manevra.UpdateTrackNote;

public class UpdateTrackNoteCommand : ICommand<bool>
{
    public int TrackId { get; set; }
    public string? Note { get; set; }
}
