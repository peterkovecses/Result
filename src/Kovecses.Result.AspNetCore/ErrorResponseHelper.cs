using Microsoft.AspNetCore.Http;

namespace Kovecses.Result.AspNetCore;

internal static class ErrorResponseHelper
{
    internal static int GetStatusCode(ErrorType type) => type switch
    {
        ErrorType.Validation => StatusCodes.Status400BadRequest,
        ErrorType.Failure => StatusCodes.Status400BadRequest,
        ErrorType.NotFound => StatusCodes.Status404NotFound,
        ErrorType.Conflict => StatusCodes.Status409Conflict,
        ErrorType.Unauthorized => StatusCodes.Status401Unauthorized,
        ErrorType.Forbidden => StatusCodes.Status403Forbidden,
        ErrorType.Timeout => StatusCodes.Status408RequestTimeout,
        ErrorType.Unexpected => StatusCodes.Status500InternalServerError,
        ErrorType.Canceled => StatusCodes.Status400BadRequest,
        _ => StatusCodes.Status400BadRequest
    };

    internal static string GetTitle(ErrorType type) => type switch
    {
        ErrorType.Validation => "Validation Error",
        ErrorType.NotFound => "Not Found",
        ErrorType.Conflict => "Conflict",
        ErrorType.Unauthorized => "Unauthorized",
        ErrorType.Forbidden => "Forbidden",
        ErrorType.Timeout => "Request Timeout",
        ErrorType.Unexpected => "Internal Server Error",
        ErrorType.Canceled => "Operation Canceled",
        _ => "Bad Request"
    };

    internal static IDictionary<string, string[]> GetValidationDictionary(Error[] errors)
    {
        var result = new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase);

        foreach (var error in errors)
        {
            if (error.Code != ErrorCodes.Validation)
            {
                AddMessages(result, error.Code, [error.Message]);
            }

            if (error.Metadata is not null)
            {
                foreach (var (key, value) in error.Metadata)
                {
                    AddMessages(result, key, ExtractMessages(value));
                }
            }
        }

        return result.ToDictionary(k => k.Key, v => v.Value.ToArray());
    }

    private static void AddMessages(Dictionary<string, List<string>> result, string key, IEnumerable<string> messages)
    {
        if (!result.TryGetValue(key, out var list))
        {
            list = [];
            result[key] = list;
        }

        list.AddRange(messages);
    }

    private static List<string> ExtractMessages(object? value) => value switch
    {
        null => [],
        string s => [s],
        IEnumerable<string> stringList => [.. stringList],
        System.Collections.IEnumerable enumerable => [.. enumerable.Cast<object>().Select(e => e?.ToString() ?? string.Empty)],
        _ => [value.ToString() ?? string.Empty]
    };

    internal static Dictionary<string, object?>? GetExtensions(Error[] errors, Error firstError)
    {
        if (errors.Length > 1)
        {
            return new Dictionary<string, object?> { ["errors"] = errors };
        }

        if (firstError.Metadata is null)
        {
            return null;
        }

        var extensions = new Dictionary<string, object?>();

        foreach (var (key, value) in firstError.Metadata)
        {
            extensions[key] = value;
        }

        return extensions;
    }
}
