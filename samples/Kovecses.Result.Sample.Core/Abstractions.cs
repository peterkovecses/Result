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
/// Interface for validating a specific request before it reaches the handler.
/// </summary>
public interface IValidator<in TRequest>
{
    Task<Result> ValidateAsync(TRequest request, CancellationToken cancellationToken);
}

/// <summary>
/// A simple mediator to decouple requests from their handlers and add cross-cutting concerns like validation.
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
        var requestType = request.GetType();

        // 1. Validation Step
        var validatorType = typeof(IValidator<>).MakeGenericType(requestType);
        var validator = serviceProvider.GetService(validatorType);
        
        if (validator is not null)
        {
            var validateMethod = validatorType.GetMethod(nameof(IValidator<object>.ValidateAsync));
            var validationResult = await (Task<Result>)validateMethod!.Invoke(validator, [request, cancellationToken])!;
            
            if (validationResult.IsFailure)
            {
                if (!typeof(TResult).IsGenericType) return (TResult)validationResult;
                var dataType = typeof(TResult).GetGenericArguments()[0];
                return (TResult)typeof(Result).GetMethods()
                    .First(m => m.Name == nameof(Result.Failure) && m.IsGenericMethod && m.GetParameters().Length == 1 && m.GetParameters()[0].ParameterType == typeof(Error))
                    .MakeGenericMethod(dataType)
                    .Invoke(null, [validationResult.Error])!;
            }
        }

        // 2. Execution Step
        var handlerType = typeof(IRequestHandler<,>).MakeGenericType(requestType, typeof(TResult));
        var handler = serviceProvider.GetService(handlerType) 
                      ?? throw new InvalidOperationException($"No handler registered for {requestType.Name}");

        var method = handlerType.GetMethod(nameof(IRequestHandler<IRequest<TResult>, TResult>.HandleAsync));
        
        return await (Task<TResult>)method!.Invoke(handler, [request, cancellationToken])!;
    }
}
