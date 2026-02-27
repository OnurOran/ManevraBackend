using MyApp.Api.Common.Behaviors;

namespace MyApp.Api.Features.Manevra.ApproveTransfer;

public class ApproveTransferCommand : ICommand<bool>
{
    public int TransferId { get; set; }
}
