namespace Kovecses.Result.Sample.BookStore.Core.Application;

/// <summary>
/// A command or query that produces a <see cref="Result"/> (or a <see cref="Result{TData}"/>).
/// </summary>
public interface IRequest<TResult> where TResult : Result;

/// <summary>
/// Handles a single request type. Handlers can be injected directly or dispatched through <see cref="IMediator"/>.
/// </summary>
public interface IRequestHandler<in TRequest, TResult>
    where TRequest : IRequest<TResult>
    where TResult : Result
{
    Task<TResult> HandleAsync(TRequest request, CancellationToken cancellationToken);
}

/// <summary>
/// Validates a request before its handler runs. Requests without a validator are passed straight through.
/// </summary>
public interface IValidator<in TRequest>
{
    Task<Result> ValidateAsync(TRequest request, CancellationToken cancellationToken);
}

/// <summary>
/// Dispatches a request to the handler registered for it.
/// </summary>
public interface IMediator
{
    Task<TResult> SendAsync<TRequest, TResult>(TRequest request, CancellationToken cancellationToken = default)
        where TRequest : IRequest<TResult>
        where TResult : Result;
}

/// <summary>
/// Resolves the (validated) handler for the request type. No reflection involved: the generic arguments are known at compile time.
/// </summary>
public sealed class Mediator(IServiceProvider serviceProvider) : IMediator
{
    public Task<TResult> SendAsync<TRequest, TResult>(TRequest request, CancellationToken cancellationToken = default)
        where TRequest : IRequest<TResult>
        where TResult : Result
        => serviceProvider
            .GetRequiredService<IRequestHandler<TRequest, TResult>>()
            .HandleAsync(request, cancellationToken);
}

/// <summary>
/// Runs every registered <see cref="IValidator{TRequest}"/> before the inner handler.
/// </summary>
internal sealed class ValidationDecorator<TRequest, TResult>(
    IRequestHandler<TRequest, TResult> inner,
    IEnumerable<IValidator<TRequest>> validators) : IRequestHandler<TRequest, TResult>
    where TRequest : IRequest<TResult>
    where TResult : Result
{
    public async Task<TResult> HandleAsync(TRequest request, CancellationToken cancellationToken)
    {
        var results = new List<Result>();

        foreach (var validator in validators)
        {
            results.Add(await validator.ValidateAsync(request, cancellationToken));
        }

        var validation = Result.Combine(results.ToArray());

        // TResult is either Result or Result<T>, CreateFailure builds the right one.
        return validation.IsFailure
            ? Result.CreateFailure<TResult>(validation.Errors!)
            : await inner.HandleAsync(request, cancellationToken);
    }
}
