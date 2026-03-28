using System.Text.Json.Serialization;

namespace Kovecses.Result;

/// <summary>
/// Defines the technical categories of errors to facilitate consistent handling (e.g., HTTP mapping).
/// </summary>
public enum ErrorType
{
    /// <summary>
    /// A general failure that doesn't fit into other categories. (Maps to HTTP 400 BadRequest by default)
    /// </summary>
    Failure = 0,

    /// <summary>
    /// An unexpected internal error. (Maps to HTTP 500 InternalServerError)
    /// </summary>
    Unexpected = 1,

    /// <summary>
    /// A validation error representing invalid input. (Maps to HTTP 400 BadRequest)
    /// </summary>
    Validation = 2,

    /// <summary>
    /// A conflict with the current state of the resource. (Maps to HTTP 409 Conflict)
    /// </summary>
    Conflict = 3,

    /// <summary>
    /// The requested resource was not found. (Maps to HTTP 404 NotFound)
    /// </summary>
    NotFound = 4,

    /// <summary>
    /// Authentication is required or has failed. (Maps to HTTP 401 Unauthorized)
    /// </summary>
    Unauthorized = 5,

    /// <summary>
    /// The authenticated user does not have permission. (Maps to HTTP 403 Forbidden)
    /// </summary>
    Forbidden = 6,

    /// <summary>
    /// The operation has timed out. (Maps to HTTP 408 RequestTimeout)
    /// </summary>
    Timeout = 7
}

/// <summary>
/// Represents a domain error with a specific code, message, technical type, and optional metadata.
/// </summary>
public record Error
{
    /// <summary>
    /// Gets the unique business error code (e.g., "User.NotFound").
    /// </summary>
    public string Code { get; init; }

    /// <summary>
    /// Gets the human-readable error message.
    /// </summary>
    public string Message { get; init; }

    /// <summary>
    /// Gets the technical type of the error for infrastructure handling.
    /// </summary>
    public ErrorType Type { get; init; }

    /// <summary>
    /// Gets the optional metadata associated with the error (e.g., validation details).
    /// </summary>
    public Dictionary<string, object>? Metadata { get; init; }

    /// <summary>
    /// Initializes a new instance of the <see cref="Error"/> record.
    /// </summary>
    /// <param name="code">The unique business error code.</param>
    /// <param name="message">The human-readable error message.</param>
    /// <param name="type">The technical type of the error.</param>
    /// <param name="metadata">Optional metadata associated with the error.</param>
    [JsonConstructor]
    public Error(string code, string message, ErrorType type, Dictionary<string, object>? metadata = null)
    {
        Code = code;
        Message = message;
        Type = type;
        Metadata = metadata;
    }

    /// <summary>
    /// Creates a custom error with a specific type.
    /// </summary>
    public static Error Custom(string code, string message, ErrorType type, Dictionary<string, object>? metadata = null)
        => new(code, message, type, metadata);

    /// <summary>
    /// Creates a "Validation" error.
    /// </summary>
    public static Error Validation(Dictionary<string, object> errors, string message = "One or more validation errors occurred.", string code = ErrorCodes.Validation)
        => new(code, message, ErrorType.Validation, errors);

    /// <summary>
    /// Creates a "NotFound" error.
    /// </summary>
    public static Error NotFound(string message = "The requested resource was not found.", string code = ErrorCodes.NotFound)
        => new(code, message, ErrorType.NotFound);

    /// <summary>
    /// Creates an "Unauthorized" error.
    /// </summary>
    public static Error Unauthorized(string message = "Authentication is required to access this resource.", string code = ErrorCodes.Unauthorized)
        => new(code, message, ErrorType.Unauthorized);

    /// <summary>
    /// Creates a "Forbidden" error.
    /// </summary>
    public static Error Forbidden(string message = "You do not have permission to access this resource.", string code = ErrorCodes.Forbidden)
        => new(code, message, ErrorType.Forbidden);

    /// <summary>
    /// Creates a "Conflict" error.
    /// </summary>
    public static Error Conflict(string message = "A conflict occurred while processing the request.", string code = ErrorCodes.Conflict)
        => new(code, message, ErrorType.Conflict);

    /// <summary>
    /// Creates a "BadRequest" (Failure) error.
    /// </summary>
    public static Error Failure(string message = "The request was invalid or cannot be served.", string code = ErrorCodes.Failure)
        => new(code, message, ErrorType.Failure);

    /// <summary>
    /// Creates a "Timeout" error.
    /// </summary>
    public static Error Timeout(string message = "The operation has timeout.", string code = ErrorCodes.Timeout)
        => new(code, message, ErrorType.Timeout);

    /// <summary>
    /// Creates an "Unexpected" internal error.
    /// </summary>
    public static Error Unexpected(string message = "An unexpected error occurred while processing your request.", string code = ErrorCodes.Unexpected)
        => new(code, message, ErrorType.Unexpected);
}
