using MyApp.Api.Common.Models;

namespace MyApp.Api.Common.Behaviors;

public interface IQuery<TResult> { }

public interface IQueryHandler<TQuery, TResult>
{
    Task<Result<TResult>> Handle(TQuery query, CancellationToken cancellationToken);
}
