using RealtorApi.Infrastructure.Results;

namespace RealtorApi.Infrastructure.Handlers;

public interface IHandler;

public interface IHandler<in TRequest, TResult> : IHandler
{
    Task<Result<TResult>> HandleAsync(TRequest request, CancellationToken cancellationToken);
}
