using MyApp.Api.Common.Behaviors;

namespace MyApp.Api.Features.Manevra.RejectTransfer;

public class RejectTransferCommand : ICommand<bool>
{
    public int TransferId { get; set; }
}
