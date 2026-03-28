namespace Kovecses.Result.Sample.Core;

/// <summary>
/// A simple employee model for the sample.
/// </summary>
public record Employee(int Id, string FullName, string Position);

/// <summary>
/// Marker interface for a request that returns a Result.
/// </summary>
public interface IRequest<TResult> where TResult : Result;

/// <summary>
/// Interface for handling a request.
/// </summary>
public interface IRequestHandler<in TRequest, TResult>
    where TRequest : IRequest<TResult>
    where TResult : Result
{
    Task<TResult> HandleAsync(TRequest request, CancellationToken cancellationToken);
}

/// <summary>
/// A simple mediator to decouple requests from their handlers.
/// </summary>
public interface IMediator
{
    Task<TResult> SendAsync<TResult>(IRequest<TResult> request, CancellationToken cancellationToken = default)
        where TResult : Result;
}

public class Mediator(IServiceProvider serviceProvider) : IMediator
{
    public async Task<TResult> SendAsync<TResult>(IRequest<TResult> request, CancellationToken cancellationToken = default)
        where TResult : Result
    {
        var handlerType = typeof(IRequestHandler<,>).MakeGenericType(request.GetType(), typeof(TResult));
        var handler = serviceProvider.GetService(handlerType) ?? throw new InvalidOperationException($"No handler registered for {request.GetType().Name}");
        var method = handlerType.GetMethod(nameof(IRequestHandler<IRequest<TResult>, TResult>.HandleAsync));

        return await (Task<TResult>)method!.Invoke(handler, [request, cancellationToken])!;
    }
}
