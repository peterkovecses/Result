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
    [JsonConstructor]
    public Result(Error? error)
    {
        Error = error;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Result"/> class for success.
    /// </summary>
    protected Result() { }

    /// <summary>
    /// Creates a successful result.
    /// </summary>
    /// <returns>A successful <see cref="Result"/>.</returns>
    public static Result Success() => new();

    /// <summary>
    /// Creates a successful result with data.
    /// </summary>
    /// <typeparam name="TData">The type of the data.</typeparam>
    /// <param name="data">The data to return.</param>
    /// <returns>A successful <see cref="Result{TData}"/>.</returns>
    public static Result<TData> Success<TData>(TData data) => new(data, null);

    /// <summary>
    /// Creates a failed result from an <see cref="Error"/>.
    /// </summary>
    /// <param name="error">The error.</param>
    /// <returns>A failed <see cref="Result"/>.</returns>
    public static Result Failure(Error error) => new(error);

    /// <summary>
    /// Creates a failed result from a code and message.
    /// </summary>
    /// <param name="code">The error code.</param>
    /// <param name="message">The error message.</param>
    /// <returns>A failed <see cref="Result"/>.</returns>
    public static Result Failure(string code, string message) => new(new Error(code, message, ErrorType.Failure));

    /// <summary>
    /// Creates a failed result with data.
    /// </summary>
    /// <typeparam name="TData">The type of the data.</typeparam>
    /// <param name="error">The error.</param>
    /// <returns>A failed <see cref="Result{TData}"/>.</returns>
    public static Result<TData> Failure<TData>(Error error) => new(default, error);

    /// <summary>
    /// Creates a failed result with data from a code and message.
    /// </summary>
    /// <typeparam name="TData">The type of the data.</typeparam>
    /// <param name="code">The error code.</param>
    /// <param name="message">The error message.</param>
    /// <returns>A failed <see cref="Result{TData}"/>.</returns>
    public static Result<TData> Failure<TData>(string code, string message) => new(default, new Error(code, message, ErrorType.Failure));

    /// <summary>
    /// Implicitly converts an <see cref="Error"/> to a failed <see cref="Result"/>.
    /// </summary>
    /// <param name="error">The error.</param>
    public static implicit operator Result(Error error) => Failure(error);
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
    /// <param name="error">The error if the operation failed, otherwise null.</param>
    [JsonConstructor]
    public Result(TData? data, Error? error) : base(error)
    {
        Data = data;
    }

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
}
