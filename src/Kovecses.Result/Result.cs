using System.Text.Json.Serialization;

namespace Kovecses.Result;

/// <summary>
/// Represents the result of an operation, containing either success status or a collection of errors.
/// </summary>
public class Result
{
    /// <summary>
    /// Gets the errors associated with the failure, or null if the operation was successful.
    /// </summary>
    public IReadOnlyList<Error>? Errors { get; init; }

    /// <summary>
    /// Gets the optional metadata associated with the result.
    /// </summary>
    public Dictionary<string, object>? Metadata { get; init; }

    /// <summary>
    /// Gets a value indicating whether the operation was successful.
    /// </summary>
    [JsonIgnore]
    public bool IsSuccess => Errors is null || Errors.Count == 0;

    /// <summary>
    /// Gets a value indicating whether the operation failed.
    /// </summary>
    [JsonIgnore]
    public bool IsFailure => !IsSuccess;

    /// <summary>
    /// Initializes a new instance of the <see cref="Result"/> class.
    /// </summary>
    /// <param name="errors">The errors if the operation failed, otherwise null.</param>
    /// <param name="metadata">Optional metadata associated with the result.</param>
    protected Result(IReadOnlyList<Error>? errors, Dictionary<string, object>? metadata = null)
    {
        Errors = errors;
        Metadata = metadata;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Result"/> class for serialization.
    /// </summary>
    [JsonConstructor]
    protected Result() { }

    /// <summary>
    /// Internal factory method to create a <see cref="Result"/> instance.
    /// </summary>
    internal static Result Create(IReadOnlyList<Error>? errors, Dictionary<string, object>? metadata) => new()
    {
        Errors = errors,
        Metadata = metadata
    };

    /// <summary>
    /// Creates a successful result.
    /// </summary>
    /// <param name="metadata">Optional metadata.</param>
    /// <returns>A successful <see cref="Result"/>.</returns>
    public static Result Success(Dictionary<string, object>? metadata = null) 
        => Create(null, metadata);

    /// <summary>
    /// Creates a successful result with data.
    /// </summary>
    /// <typeparam name="TData">The type of the data.</typeparam>
    /// <param name="data">The data to return.</param>
    /// <param name="metadata">Optional metadata.</param>
    /// <returns>A successful <see cref="Result{TData}"/>.</returns>
    public static Result<TData> Success<TData>(TData data, Dictionary<string, object>? metadata = null) 
        => Result<TData>.Create(data, null, metadata);

    /// <summary>
    /// Creates a failed result from a single error.
    /// </summary>
    /// <param name="error">The error.</param>
    /// <param name="metadata">Optional metadata.</param>
    /// <returns>A failed <see cref="Result"/>.</returns>
    public static Result Failure(Error error, Dictionary<string, object>? metadata = null) 
        => Create([error], metadata);

    /// <summary>
    /// Creates a failed result from a code and message.
    /// </summary>
    /// <param name="code">The error code.</param>
    /// <param name="message">The error message.</param>
    /// <param name="metadata">Optional metadata.</param>
    /// <returns>A failed <see cref="Result"/>.</returns>
    public static Result Failure(string code, string message, Dictionary<string, object>? metadata = null) 
        => Failure(new Error(code, message, ErrorType.Failure), metadata);

    /// <summary>
    /// Creates a failed result from a collection of errors.
    /// </summary>
    /// <param name="errors">The collection of errors.</param>
    /// <param name="metadata">Optional metadata.</param>
    /// <returns>A failed <see cref="Result"/>.</returns>
    public static Result Failure(IEnumerable<Error> errors, Dictionary<string, object>? metadata = null) 
        => Create([.. errors], metadata);

    /// <summary>
    /// Creates a failed result with data from a single error.
    /// </summary>
    /// <typeparam name="TData">The type of the data.</typeparam>
    /// <param name="error">The error.</param>
    /// <param name="metadata">Optional metadata.</param>
    /// <returns>A failed <see cref="Result{TData}"/>.</returns>
    public static Result<TData> Failure<TData>(Error error, Dictionary<string, object>? metadata = null) 
        => Result<TData>.Create(default, [error], metadata);

    /// <summary>
    /// Creates a failed result with data from a code and message.
    /// </summary>
    /// <typeparam name="TData">The type of the data.</typeparam>
    /// <param name="code">The error code.</param>
    /// <param name="message">The error message.</param>
    /// <param name="metadata">Optional metadata.</param>
    /// <returns>A failed <see cref="Result{TData}"/>.</returns>
    public static Result<TData> Failure<TData>(string code, string message, Dictionary<string, object>? metadata = null) 
        => Failure<TData>(new Error(code, message, ErrorType.Failure), metadata);

    /// <summary>
    /// Creates a failed result with data from a collection of errors.
    /// </summary>
    /// <typeparam name="TData">The type of the data.</typeparam>
    /// <param name="errors">The collection of errors.</param>
    /// <param name="metadata">Optional metadata.</param>
    /// <returns>A failed <see cref="Result{TData}"/>.</returns>
    public static Result<TData> Failure<TData>(IEnumerable<Error> errors, Dictionary<string, object>? metadata = null) 
        => Result<TData>.Create(default, [.. errors], metadata);

    /// <summary>
    /// Creates a failure response of the specified type using an optimized factory.
    /// </summary>
    /// <typeparam name="TResponse">The type of the response, must be a <see cref="Result"/>.</typeparam>
    /// <param name="errors">The collection of errors.</param>
    /// <returns>A failed result of type <typeparamref name="TResponse"/>.</returns>
    public static TResponse CreateFailure<TResponse>(IReadOnlyList<Error> errors) where TResponse : Result 
        => FailureFactory<TResponse>.Create(errors);

    /// <summary>
    /// Implicitly converts an <see cref="Error"/> to a failed <see cref="Result"/>.
    /// </summary>
    /// <param name="error">The error.</param>
    public static implicit operator Result(Error error) 
        => Failure(error);

    /// <summary>
    /// Executes the onSuccess function if the result is successful, otherwise executes the onFailure function.
    /// </summary>
    /// <typeparam name="TResult">The type of the result after matching.</typeparam>
    /// <param name="onSuccess">The function to execute on success.</param>
    /// <param name="onFailure">The function to execute on failure with the collection of errors.</param>
    /// <returns>The result of the executed function.</returns>
    public TResult Match<TResult>(Func<TResult> onSuccess, Func<IReadOnlyList<Error>, TResult> onFailure)
        => IsSuccess 
            ? onSuccess() 
            : onFailure(Errors!);

    /// <summary>
    /// Asynchronously executes the onSuccess function if the result is successful, otherwise executes the onFailure function.
    /// </summary>
    /// <typeparam name="TResult">The type of the result after matching.</typeparam>
    /// <param name="onSuccess">The function to execute on success.</param>
    /// <param name="onFailure">The function to execute on failure with the collection of errors.</param>
    /// <returns>A task representing the result of the executed function.</returns>
    public async Task<TResult> MatchAsync<TResult>(Func<Task<TResult>> onSuccess, Func<IReadOnlyList<Error>, Task<TResult>> onFailure)
        => IsSuccess 
            ? await onSuccess() 
            : await onFailure(Errors!);

    /// <summary>
    /// Chains another operation if the current result is successful.
    /// </summary>
    /// <param name="next">The next operation to execute.</param>
    /// <returns>The result of the next operation, or the current failure.</returns>
    public Result Bind(Func<Result> next)
        => IsSuccess 
            ? next() 
            : this;

    /// <summary>
    /// Asynchronously chains another operation if the current result is successful.
    /// </summary>
    /// <param name="next">The next operation to execute.</param>
    /// <returns>A task representing the result of the next operation, or the current failure.</returns>
    public async Task<Result> BindAsync(Func<Task<Result>> next)
        => IsSuccess 
            ? await next() 
            : this;

    /// <summary>
    /// Executes an action if the result is successful.
    /// </summary>
    /// <param name="action">The action to execute.</param>
    /// <returns>The current result.</returns>
    public Result Tap(Action action)
    {
        if (IsSuccess) action();

        return this;
    }

    /// <summary>
    /// Asynchronously executes a function if the result is successful.
    /// </summary>
    /// <param name="func">The asynchronous function to execute.</param>
    /// <returns>A task representing the current result.</returns>
    public async Task<Result> TapAsync(Func<Task> func)
    {
        if (IsSuccess) await func();
        
        return this;
    }

    /// <summary>
    /// Combines multiple results into a single result, merging all errors if any failures occur.
    /// </summary>
    /// <param name="results">The results to combine.</param>
    /// <returns>A successful result if all inputs are successful, otherwise a failed result containing all merged errors.</returns>
    public static Result Combine(params Result[] results)
    {
        var failures = results
            .Where(r => r.IsFailure)
            .SelectMany(r => r.Errors!)
            .ToList();

        if (failures.Count == 0)
        {
            return Success();
        }

        return Failure(failures);
    }

    private static class FailureFactory<TResponse> where TResponse : Result
    {
        public static readonly Func<IReadOnlyList<Error>, TResponse> Create;

        static FailureFactory()
        {
            var responseType = typeof(TResponse);

            if (responseType == typeof(Result))
            {
                Create = errs => (TResponse)(object)Failure(errs);
                
                return;
            }

            if (responseType.IsGenericType && responseType.GetGenericTypeDefinition() == typeof(Result<>))
            {
                var dataType = responseType.GetGenericArguments()[0];
                var method = typeof(Result).GetMethods(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static)
                    .First(m => m.Name == nameof(Failure) && 
                                m.IsGenericMethod && 
                                m.GetParameters().Length == 2 && 
                                m.GetParameters()[0].ParameterType == typeof(IEnumerable<Error>) &&
                                m.GetParameters()[1].ParameterType == typeof(Dictionary<string, object>))
                    .MakeGenericMethod(dataType);

                var func = (Func<IEnumerable<Error>, Dictionary<string, object>?, TResponse>)Delegate.CreateDelegate(
                    typeof(Func<IEnumerable<Error>, Dictionary<string, object>?, TResponse>), method);

                Create = errs => func(errs, null);
                
                return;
            }

            throw new InvalidOperationException($"Cannot create failure response for type {responseType.FullName}. Only Result and Result<T> are supported.");
        }
    }
}

/// <summary>
/// Represents the result of an operation that returns a value on success.
/// </summary>
/// <typeparam name="TData">The type of the data returned on success.</typeparam>
public class Result<TData> : Result
{
    /// <summary>
    /// Gets the data returned on success, or null if the operation failed.
    /// </summary>
    public TData? Data { get; init; }

    /// <summary>
    /// Initializes a new instance of the <see cref="Result{TData}"/> class.
    /// </summary>
    /// <param name="data">The data returned on success.</param>
    /// <param name="errors">The errors if the operation failed, otherwise null.</param>
    /// <param name="metadata">Optional metadata associated with the result.</param>
    protected Result(TData? data, IReadOnlyList<Error>? errors, Dictionary<string, object>? metadata = null) 
        : base(errors, metadata)
    {
        Data = data;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Result{TData}"/> class for serialization.
    /// </summary>
    [JsonConstructor]
    private Result() { }

    /// <summary>
    /// Internal factory method to create a <see cref="Result{TData}"/> instance.
    /// </summary>
    internal static Result<TData> Create(TData? data, IReadOnlyList<Error>? errors, Dictionary<string, object>? metadata)
    {
        return new Result<TData>
        {
            Data = data,
            Errors = errors,
            Metadata = metadata
        };
    }

    /// <summary>
    /// Implicitly converts data to a successful <see cref="Result{TData}"/>.
    /// </summary>
    /// <param name="data">The data.</param>
    public static implicit operator Result<TData>(TData data) 
        => Success(data);

    /// <summary>
    /// Implicitly converts an <see cref="Error"/> to a failed <see cref="Result{TData}"/>.
    /// </summary>
    /// <param name="error">The error.</param>
    public static implicit operator Result<TData>(Error error) 
        => Failure<TData>(error);

    /// <summary>
    /// Executes the onSuccess function if the result is successful, otherwise executes the onFailure function.
    /// </summary>
    /// <typeparam name="TResult">The type of the result after matching.</typeparam>
    /// <param name="onSuccess">The function to execute on success with the data.</param>
    /// <param name="onFailure">The function to execute on failure with the collection of errors.</param>
    /// <returns>The result of the executed function.</returns>
    public TResult Match<TResult>(Func<TData, TResult> onSuccess, Func<IReadOnlyList<Error>, TResult> onFailure)
        => IsSuccess 
            ? onSuccess(Data!) 
            : onFailure(Errors!);

    /// <summary>
    /// Asynchronously executes the onSuccess function if the result is successful, otherwise executes the onFailure function.
    /// </summary>
    /// <typeparam name="TResult">The type of the result after matching.</typeparam>
    /// <param name="onSuccess">The function to execute on success with the data.</param>
    /// <param name="onFailure">The function to execute on failure with the collection of errors.</param>
    /// <returns>A task representing the result of the executed function.</returns>
    public async Task<TResult> MatchAsync<TResult>(Func<TData, Task<TResult>> onSuccess, Func<IReadOnlyList<Error>, Task<TResult>> onFailure)
        => IsSuccess 
            ? await onSuccess(Data!) 
            : await onFailure(Errors!);

    /// <summary>
    /// Transforms the success value of the result.
    /// </summary>
    /// <typeparam name="TNewData">The type of the transformed data.</typeparam>
    /// <param name="map">The mapping function.</param>
    /// <returns>A new result with the transformed data, or the current failure.</returns>
    public Result<TNewData> Map<TNewData>(Func<TData, TNewData> map)
        => IsSuccess 
            ? Success(map(Data!)) 
            : Failure<TNewData>(Errors!);

    /// <summary>
    /// Asynchronously transforms the success value of the result.
    /// </summary>
    /// <typeparam name="TNewData">The type of the transformed data.</typeparam>
    /// <param name="map">The asynchronous mapping function.</param>
    /// <returns>A task representing the new result with the transformed data, or the current failure.</returns>
    public async Task<Result<TNewData>> MapAsync<TNewData>(Func<TData, Task<TNewData>> map)
        => IsSuccess 
            ? Success(await map(Data!)) 
            : Failure<TNewData>(Errors!);

    /// <summary>
    /// Chains another result-returning operation if the current result is successful.
    /// </summary>
    /// <typeparam name="TNewData">The type of the new result data.</typeparam>
    /// <param name="next">The next operation to execute.</param>
    /// <returns>The result of the next operation, or the current failure.</returns>
    public Result<TNewData> Bind<TNewData>(Func<TData, Result<TNewData>> next)
        => IsSuccess 
            ? next(Data!) 
            : Failure<TNewData>(Errors!);

    /// <summary>
    /// Asynchronously chains another result-returning operation if the current result is successful.
    /// </summary>
    /// <typeparam name="TNewData">The type of the new result data.</typeparam>
    /// <param name="next">The next asynchronous operation to execute.</param>
    /// <returns>A task representing the result of the next operation, or the current failure.</returns>
    public async Task<Result<TNewData>> BindAsync<TNewData>(Func<TData, Task<Result<TNewData>>> next)
        => IsSuccess 
            ? await next(Data!) 
            : Failure<TNewData>(Errors!);

    /// <summary>
    /// Executes an action with the data if the result is successful.
    /// </summary>
    /// <param name="action">The action to execute.</param>
    /// <returns>The current result.</returns>
    public Result<TData> Tap(Action<TData> action)
    {
        if (IsSuccess)
        {
            action(Data!);
        }
        
        return this;
    }

    /// <summary>
    /// Asynchronously executes a function with the data if the result is successful.
    /// </summary>
    /// <param name="func">The asynchronous function to execute.</param>
    /// <returns>A task representing the current result.</returns>
    public async Task<Result<TData>> TapAsync(Func<TData, Task> func)
    {
        if (IsSuccess)
        {
            await func(Data!);
        }

        return this;
    }

    /// <summary>
    /// Returns the data if the result is successful, otherwise throws an exception.
    /// </summary>
    /// <param name="exceptionFactory">An optional factory to create the exception to throw. If null, an <see cref="InvalidOperationException"/> is thrown.</param>
    /// <returns>The data contained in the result.</returns>
    /// <exception cref="InvalidOperationException">Thrown if the result is a failure and no exception factory is provided.</exception>
    public TData ValueOrThrow(Func<IReadOnlyList<Error>, Exception>? exceptionFactory = null)
    {
        if (IsSuccess) return Data!;
        
        throw exceptionFactory?.Invoke(Errors!) 
              ?? new InvalidOperationException($"Cannot access data of a failure result. Errors: {string.Join(", ", Errors!.Select(e => $"{e.Code}: {e.Message}"))}");
    }

    /// <summary>
    /// Returns the data if the result is successful, otherwise returns the provided default value.
    /// </summary>
    /// <param name="defaultValue">The value to return if the result is a failure.</param>
    /// <returns>The data or the default value.</returns>
    public TData? ValueOrDefault(TData? defaultValue = default)
        => IsSuccess 
            ? Data 
            : defaultValue;
}
