using MyApp.Api.Common.Models;

namespace MyApp.Api.Common.Behaviors;

public interface ICommand<TResult> { }

public interface ICommandHandler<TCommand, TResult>
{
    Task<Result<TResult>> Handle(TCommand command, CancellationToken cancellationToken);
}
