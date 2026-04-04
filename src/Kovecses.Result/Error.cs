using System.Text.Json;
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
    Timeout = 7,

    /// <summary>
    /// The operation was canceled. (Maps to HTTP 400 BadRequest or custom handling)
    /// </summary>
    Canceled = 8
}

/// <summary>
/// Represents a domain error with a specific code, message, technical type, and optional metadata.
/// </summary>
[JsonConverter(typeof(ErrorJsonConverter))]
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
    /// <exception cref="ArgumentNullException">Thrown when code or message is null.</exception>
    public Error(string code, string message, ErrorType type, Dictionary<string, object>? metadata = null)
    {
        Code = code ?? throw new ArgumentNullException(nameof(code));
        Message = message ?? throw new ArgumentNullException(nameof(message));
        Type = type;
        Metadata = metadata;
    }

    /// <summary>
    /// Creates a custom error with a specific type.
    /// </summary>
    /// <param name="code">The error code.</param>
    /// <param name="message">The error message.</param>
    /// <param name="type">The error type.</param>
    /// <param name="metadata">Optional metadata.</param>
    /// <returns>A new <see cref="Error"/> instance.</returns>
    public static Error Custom(string code, string message, ErrorType type, Dictionary<string, object>? metadata = null)
        => new(code, message, type, metadata);

    /// <summary>
    /// Creates a "Validation" error.
    /// </summary>
    /// <param name="code">The error code.</param>
    /// <param name="message">The error message.</param>
    /// <param name="metadata">Optional metadata associated with the validation error (e.g., field-level details).</param>
    /// <returns>A new validation <see cref="Error"/>.</returns>
    public static Error Validation(string code, string message, Dictionary<string, object>? metadata = null)
        => new(code, message, ErrorType.Validation, metadata);

    /// <summary>
    /// Creates a "NotFound" error.
    /// </summary>
    /// <param name="message">The error message.</param>
    /// <param name="code">The error code.</param>
    /// <returns>A new not found <see cref="Error"/>.</returns>
    public static Error NotFound(string message = "The requested resource was not found.", string code = ErrorCodes.NotFound)
        => new(code, message, ErrorType.NotFound);

    /// <summary>
    /// Creates an "Unauthorized" error.
    /// </summary>
    /// <param name="message">The error message.</param>
    /// <param name="code">The error code.</param>
    /// <returns>A new unauthorized <see cref="Error"/>.</returns>
    public static Error Unauthorized(string message = "Authentication is required to access this resource.", string code = ErrorCodes.Unauthorized)
        => new(code, message, ErrorType.Unauthorized);

    /// <summary>
    /// Creates a "Forbidden" error.
    /// </summary>
    /// <param name="message">The error message.</param>
    /// <param name="code">The error code.</param>
    /// <returns>A new forbidden <see cref="Error"/>.</returns>
    public static Error Forbidden(string message = "You do not have permission to access this resource.", string code = ErrorCodes.Forbidden)
        => new(code, message, ErrorType.Forbidden);

    /// <summary>
    /// Creates a "Conflict" error.
    /// </summary>
    /// <param name="message">The error message.</param>
    /// <param name="code">The error code.</param>
    /// <returns>A new conflict <see cref="Error"/>.</returns>
    public static Error Conflict(string message = "A conflict occurred while processing the request.", string code = ErrorCodes.Conflict)
        => new(code, message, ErrorType.Conflict);

    /// <summary>
    /// Creates a "Failure" (BadRequest) error.
    /// </summary>
    /// <param name="message">The error message.</param>
    /// <param name="code">The error code.</param>
    /// <returns>A new failure <see cref="Error"/>.</returns>
    public static Error Failure(string message = "The request was invalid or cannot be served.", string code = ErrorCodes.Failure)
        => new(code, message, ErrorType.Failure);

    /// <summary>
    /// Creates a "Timeout" error.
    /// </summary>
    /// <param name="message">The error message.</param>
    /// <param name="code">The error code.</param>
    /// <returns>A new timeout <see cref="Error"/>.</returns>
    public static Error Timeout(string message = "The operation has timed out.", string code = ErrorCodes.Timeout)
        => new(code, message, ErrorType.Timeout);

    /// <summary>
    /// Creates an "Unexpected" internal error.
    /// </summary>
    /// <param name="message">The error message.</param>
    /// <param name="code">The error code.</param>
    /// <returns>A new unexpected <see cref="Error"/>.</returns>
    public static Error Unexpected(string message = "An unexpected error occurred while processing your request.", string code = ErrorCodes.Unexpected)
        => new(code, message, ErrorType.Unexpected);

    /// <summary>
    /// Creates a "Canceled" error.
    /// </summary>
    /// <param name="message">The error message.</param>
    /// <param name="code">The error code.</param>
    /// <returns>A new canceled <see cref="Error"/>.</returns>
    public static Error Canceled(string message = "The operation was canceled.", string code = ErrorCodes.Canceled)
        => new(code, message, ErrorType.Canceled);
}

/// <summary>
/// Custom JSON converter for <see cref="Error"/> to handle Metadata deserialization properly.
/// </summary>
public class ErrorJsonConverter : JsonConverter<Error>
{
    /// <inheritdoc />
    public override Error? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.Null)
            return null;

        using var doc = JsonDocument.ParseValue(ref reader);
        var root = doc.RootElement;

        var code = GetStringProperty(root, "Code", options) ?? "Unknown";
        var message = GetStringProperty(root, "Message", options) ?? "Unknown error";
        var type = GetEnumProperty<ErrorType>(root, "Type", options);
        var metadata = GetMetadataProperty(root, options);

        return new Error(code, message, type, metadata);
    }

    /// <inheritdoc />
    public override void Write(Utf8JsonWriter writer, Error value, JsonSerializerOptions options)
    {
        writer.WriteStartObject();

        writer.WriteString(GetPropertyName("Code", options), value.Code);
        writer.WriteString(GetPropertyName("Message", options), value.Message);
        writer.WriteNumber(GetPropertyName("Type", options), (int)value.Type);

        if (value.Metadata is not null && value.Metadata.Count > 0)
        {
            writer.WritePropertyName(GetPropertyName("Metadata", options));
            JsonSerializer.Serialize(writer, value.Metadata, options);
        }

        writer.WriteEndObject();
    }

    private static string? GetStringProperty(JsonElement root, string propertyName, JsonSerializerOptions options)
    {
        var name = GetPropertyName(propertyName, options);

        if (root.TryGetProperty(name, out var element) ||
            TryGetPropertyCaseInsensitive(root, propertyName, out element))
        {
            return element.GetString();
        }

        return null;
    }

    private static TEnum GetEnumProperty<TEnum>(JsonElement root, string propertyName, JsonSerializerOptions options)
        where TEnum : struct, Enum
    {
        var name = GetPropertyName(propertyName, options);

        if (root.TryGetProperty(name, out var element) ||
            TryGetPropertyCaseInsensitive(root, propertyName, out element))
        {
            if (element.ValueKind == JsonValueKind.Number)
            {
                return (TEnum)(object)element.GetInt32();
            }

            if (element.ValueKind == JsonValueKind.String &&
                Enum.TryParse<TEnum>(element.GetString(), true, out var result))
            {
                return result;
            }
        }

        return default;
    }

    private static Dictionary<string, object>? GetMetadataProperty(JsonElement root, JsonSerializerOptions options)
    {
        var name = GetPropertyName("Metadata", options);

        if (root.TryGetProperty(name, out var element) ||
            TryGetPropertyCaseInsensitive(root, "Metadata", out element))
        {
            if (element.ValueKind == JsonValueKind.Object)
            {
                return DeserializeMetadataObject(element);
            }
        }

        return null;
    }

    private static Dictionary<string, object>? DeserializeMetadataObject(JsonElement element)
    {
        var result = new Dictionary<string, object>();

        foreach (var property in element.EnumerateObject())
        {
            result[property.Name] = DeserializeValue(property.Value);
        }

        return result.Count > 0 ? result : null;
    }

    private static object DeserializeValue(JsonElement element)
    {
        return element.ValueKind switch
        {
            JsonValueKind.String => element.GetString() ?? string.Empty,
            JsonValueKind.Number when element.TryGetInt32(out var intVal) => intVal,
            JsonValueKind.Number when element.TryGetInt64(out var longVal) => longVal,
            JsonValueKind.Number when element.TryGetDouble(out var doubleVal) => doubleVal,
            JsonValueKind.True => true,
            JsonValueKind.False => false,
            JsonValueKind.Null => null!,
            JsonValueKind.Array => element.EnumerateArray().Select(DeserializeValue).ToList(),
            JsonValueKind.Object => DeserializeMetadataObject(element)!,
            _ => element.GetRawText()
        };
    }

    private static string GetPropertyName(string name, JsonSerializerOptions options) 
        => options.PropertyNamingPolicy?.ConvertName(name) ?? name;

    private static bool TryGetPropertyCaseInsensitive(JsonElement element, string propertyName, out JsonElement value)
    {
        foreach (var property in element.EnumerateObject())
        {
            if (string.Equals(property.Name, propertyName, StringComparison.OrdinalIgnoreCase))
            {
                value = property.Value;
                return true;
            }
        }

        value = default;
        
        return false;
    }
}