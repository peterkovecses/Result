using System.Text.Json.Serialization;

namespace Kovecses.Result;

/// <summary>
/// Represents the result of an operation, containing either success status or an error.
/// </summary>
public class Result
{
    /// <summary>
    /// Gets the error associated with the failure, or null if the operation was successful.
    /// </summary>
    public Error? Error { get; init; }

    /// <summary>
    /// Gets the optional metadata associated with the result.
    /// </summary>
    public Dictionary<string, object>? Metadata { get; init; }

    /// <summary>
    /// Gets a value indicating whether the operation was successful.
    /// </summary>
    [JsonIgnore]
    public bool IsSuccess => Error is null;

    /// <summary>
    /// Gets a value indicating whether the operation failed.
    /// </summary>
    [JsonIgnore]
    public bool IsFailure => !IsSuccess;

    /// <summary>
    /// Initializes a new instance of the <see cref="Result"/> class.
    /// </summary>
    /// <param name="error">The error if the operation failed, otherwise null.</param>
    /// <param name="metadata">Optional metadata associated with the result.</param>
    [JsonConstructor]
    public Result(Error? error, Dictionary<string, object>? metadata = null)
    {
        Error = error;
        Metadata = metadata;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Result"/> class for success.
    /// </summary>
    protected Result() { }

    /// <summary>
    /// Creates a successful result.
    /// </summary>
    /// <param name="metadata">Optional metadata.</param>
    /// <returns>A successful <see cref="Result"/>.</returns>
    public static Result Success(Dictionary<string, object>? metadata = null) => new(null, metadata);

    /// <summary>
    /// Creates a successful result with data.
    /// </summary>
    /// <typeparam name="TData">The type of the data.</typeparam>
    /// <param name="data">The data to return.</param>
    /// <param name="metadata">Optional metadata.</param>
    /// <returns>A successful <see cref="Result{TData}"/>.</returns>
    public static Result<TData> Success<TData>(TData data, Dictionary<string, object>? metadata = null) => new(data, null, metadata);

    /// <summary>
    /// Creates a failed result from an <see cref="Error"/>.
    /// </summary>
    /// <param name="error">The error.</param>
    /// <param name="metadata">Optional metadata.</param>
    /// <returns>A failed <see cref="Result"/>.</returns>
    public static Result Failure(Error error, Dictionary<string, object>? metadata = null) => new(error, metadata);

    /// <summary>
    /// Creates a failed result from a code and message.
    /// </summary>
    /// <param name="code">The error code.</param>
    /// <param name="message">The error message.</param>
    /// <param name="metadata">Optional metadata.</param>
    /// <returns>A failed <see cref="Result"/>.</returns>
    public static Result Failure(string code, string message, Dictionary<string, object>? metadata = null) 
        => new(new Error(code, message, ErrorType.Failure), metadata);

    /// <summary>
    /// Creates a failed result with data.
    /// </summary>
    /// <typeparam name="TData">The type of the data.</typeparam>
    /// <param name="error">The error.</param>
    /// <param name="metadata">Optional metadata.</param>
    /// <returns>A failed <see cref="Result{TData}"/>.</returns>
    public static Result<TData> Failure<TData>(Error error, Dictionary<string, object>? metadata = null) => new(default, error, metadata);

    /// <summary>
    /// Creates a failed result with data from a code and message.
    /// </summary>
    /// <typeparam name="TData">The type of the data.</typeparam>
    /// <param name="code">The error code.</param>
    /// <param name="message">The error message.</param>
    /// <param name="metadata">Optional metadata.</param>
    /// <returns>A failed <see cref="Result{TData}"/>.</returns>
    public static Result<TData> Failure<TData>(string code, string message, Dictionary<string, object>? metadata = null) 
        => new(default, new Error(code, message, ErrorType.Failure), metadata);

    /// <summary>
    /// Implicitly converts an <see cref="Error"/> to a failed <see cref="Result"/>.
    /// </summary>
    /// <param name="error">The error.</param>
    public static implicit operator Result(Error error) => Failure(error);

    /// <summary>
    /// Executes the onStatusSuccess function if the result is successful, otherwise executes the onStatusFailure function.
    /// </summary>
    /// <typeparam name="TResult">The type of the result after matching.</typeparam>
    /// <param name="onSuccess">The function to execute on success.</param>
    /// <param name="onFailure">The function to execute on failure.</param>
    /// <returns>The result of the executed function.</returns>
    public TResult Match<TResult>(Func<TResult> onSuccess, Func<Error, TResult> onFailure)
        => IsSuccess ? onSuccess() : onFailure(Error!);

    /// <summary>
    /// Asynchronously executes the onStatusSuccess function if the result is successful, otherwise executes the onStatusFailure function.
    /// </summary>
    /// <typeparam name="TResult">The type of the result after matching.</typeparam>
    /// <param name="onSuccess">The function to execute on success.</param>
    /// <param name="onFailure">The function to execute on failure.</param>
    /// <returns>A task representing the result of the executed function.</returns>
    public async Task<TResult> MatchAsync<TResult>(Func<Task<TResult>> onSuccess, Func<Error, Task<TResult>> onFailure)
        => IsSuccess ? await onSuccess() : await onFailure(Error!);

    /// <summary>
    /// Chains another operation if the current result is successful.
    /// </summary>
    /// <param name="next">The next operation to execute.</param>
    /// <returns>The result of the next operation, or the current failure.</returns>
    public Result Bind(Func<Result> next)
        => IsSuccess ? next() : this;

    /// <summary>
    /// Asynchronously chains another operation if the current result is successful.
    /// </summary>
    /// <param name="next">The next operation to execute.</param>
    /// <returns>A task representing the result of the next operation, or the current failure.</returns>
    public async Task<Result> BindAsync(Func<Task<Result>> next)
        => IsSuccess ? await next() : this;

    /// <summary>
    /// Combines multiple results into a single result.
    /// </summary>
    /// <param name="results">The results to combine.</param>
    /// <returns>A successful result if all inputs are successful, otherwise a failed result containing all errors.</returns>
    public static Result Combine(params Result[] results)
    {
        var failures = results.Where(r => r.IsFailure).ToList();
        if (failures.Count == 0)
        {
            return Success();
        }

        if (failures.Count == 1)
        {
            return failures[0];
        }

        var errors = failures
            .Select(f => f.Error!)
            .GroupBy(e => e.Code)
            .ToDictionary(
                g => g.Key,
                g => (object)g.Select(e => e.Message).ToList());

        return Error.Validation(errors, "Multiple errors occurred during the operation.");
    }
}

/// <summary>
/// Represents the result of an operation that returns a value on success.
/// </summary>
/// <typeparam name="TData">The type of the data returned on success.</typeparam>
/// <remarks>
/// Initializes a new instance of the <see cref="Result{TData}"/> class.
/// </remarks>
/// <param name="data">The data returned on success.</param>
/// <param name="error">The error if the operation failed, otherwise null.</param>
/// <param name="metadata">Optional metadata associated with the result.</param>
[method: JsonConstructor]
public class Result<TData>(TData? data, Error? error, Dictionary<string, object>? metadata = null) : Result(error, metadata)
{
    /// <summary>
    /// Gets the data returned on success, or null if the operation failed.
    /// </summary>
    public TData? Data { get; init; } = data;

    /// <summary>
    /// Implicitly converts data to a successful <see cref="Result{TData}"/>.
    /// </summary>
    /// <param name="data">The data.</param>
    public static implicit operator Result<TData>(TData data) => Success(data);

    /// <summary>
    /// Implicitly converts an <see cref="Error"/> to a failed <see cref="Result{TData}"/>.
    /// </summary>
    /// <param name="error">The error.</param>
    public static implicit operator Result<TData>(Error error) => Failure<TData>(error);

    /// <summary>
    /// Executes the onStatusSuccess function if the result is successful, otherwise executes the onStatusFailure function.
    /// </summary>
    /// <typeparam name="TResult">The type of the result after matching.</typeparam>
    /// <param name="onSuccess">The function to execute on success with the data.</param>
    /// <param name="onFailure">The function to execute on failure with the error.</param>
    /// <returns>The result of the executed function.</returns>
    public TResult Match<TResult>(Func<TData, TResult> onSuccess, Func<Error, TResult> onFailure)
        => IsSuccess ? onSuccess(Data!) : onFailure(Error!);

    /// <summary>
    /// Asynchronously executes the onStatusSuccess function if the result is successful, otherwise executes the onStatusFailure function.
    /// </summary>
    /// <typeparam name="TResult">The type of the result after matching.</typeparam>
    /// <param name="onSuccess">The function to execute on success with the data.</param>
    /// <param name="onFailure">The function to execute on failure with the error.</param>
    /// <returns>A task representing the result of the executed function.</returns>
    public async Task<TResult> MatchAsync<TResult>(Func<TData, Task<TResult>> onSuccess, Func<Error, Task<TResult>> onFailure)
        => IsSuccess ? await onSuccess(Data!) : await onFailure(Error!);

    /// <summary>
    /// Transforms the success value of the result.
    /// </summary>
    /// <typeparam name="TNewData">The type of the transformed data.</typeparam>
    /// <param name="map">The mapping function.</param>
    /// <returns>A new result with the transformed data, or the current failure.</returns>
    public Result<TNewData> Map<TNewData>(Func<TData, TNewData> map)
        => IsSuccess ? Success(map(Data!)) : Failure<TNewData>(Error!);

    /// <summary>
    /// Asynchronously transforms the success value of the result.
    /// </summary>
    /// <typeparam name="TNewData">The type of the transformed data.</typeparam>
    /// <param name="map">The asynchronous mapping function.</param>
    /// <returns>A task representing the new result with the transformed data, or the current failure.</returns>
    public async Task<Result<TNewData>> MapAsync<TNewData>(Func<TData, Task<TNewData>> map)
        => IsSuccess ? Success(await map(Data!)) : Failure<TNewData>(Error!);

    /// <summary>
    /// Chains another result-returning operation if the current result is successful.
    /// </summary>
    /// <typeparam name="TNewData">The type of the new result data.</typeparam>
    /// <param name="next">The next operation to execute.</param>
    /// <returns>The result of the next operation, or the current failure.</returns>
    public Result<TNewData> Bind<TNewData>(Func<TData, Result<TNewData>> next)
        => IsSuccess ? next(Data!) : Failure<TNewData>(Error!);

    /// <summary>
    /// Asynchronously chains another result-returning operation if the current result is successful.
    /// </summary>
    /// <typeparam name="TNewData">The type of the new result data.</typeparam>
    /// <param name="next">The next asynchronous operation to execute.</param>
    /// <returns>A task representing the result of the next operation, or the current failure.</returns>
    public async Task<Result<TNewData>> BindAsync<TNewData>(Func<TData, Task<Result<TNewData>>> next)
        => IsSuccess ? await next(Data!) : Failure<TNewData>(Error!);

    /// <summary>
    /// Returns the data if the result is successful, otherwise throws an exception.
    /// </summary>
    /// <param name="exceptionFactory">An optional factory to create the exception to throw. If null, an <see cref="InvalidOperationException"/> is thrown.</param>
    /// <returns>The data contained in the result.</returns>
    /// <exception cref="InvalidOperationException">Thrown if the result is a failure and no exception factory is provided.</exception>
    public TData ValueOrThrow(Func<Error, Exception>? exceptionFactory = null)
    {
        if (IsSuccess) return Data!;
        
        throw exceptionFactory?.Invoke(Error!) 
              ?? new InvalidOperationException($"Cannot access data of a failure result. Error: {Error!.Code} - {Error.Message}");
    }

    /// <summary>
    /// Returns the data if the result is successful, otherwise returns the provided default value.
    /// </summary>
    /// <param name="defaultValue">The value to return if the result is a failure.</param>
    /// <returns>The data or the default value.</returns>
    public TData? ValueOrDefault(TData? defaultValue = default)
        => IsSuccess ? Data : defaultValue;
}
