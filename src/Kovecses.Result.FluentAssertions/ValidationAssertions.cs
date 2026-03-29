using System.Diagnostics;
using System.Text.Json;
using Xunit;

namespace Kovecses.Result.FluentAssertions;

/// <summary>
/// Provides fluent assertions for validation errors.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="ValidationAssertions"/> class.
/// </remarks>
/// <param name="messages">The validation error messages.</param>
[StackTraceHidden]
public class ValidationAssertions(List<string> messages)
{
    private readonly List<string> _messages = messages;

    /// <summary>
    /// Asserts that at least one validation message contains the specified substring.
    /// </summary>
    /// <param name="substring">The substring to search for.</param>
    /// <returns>The <see cref="ValidationAssertions"/> for further assertions.</returns>
    public ValidationAssertions Contain(string substring)
    {
        Assert.Contains(_messages, m => m.Contains(substring, StringComparison.OrdinalIgnoreCase));
        
        return this;
    }

    /// <summary>
    /// Asserts that the validation messages contain all the specified substrings.
    /// </summary>
    /// <param name="substrings">The substrings to search for.</param>
    /// <returns>The <see cref="ValidationAssertions"/> for further assertions.</returns>
    public ValidationAssertions ContainAll(params string[] substrings)
    {
        foreach (var substring in substrings)
        {
            Contain(substring);
        }
        
        return this;
    }

    /// <summary>
    /// Gets the underlying validation messages.
    /// </summary>
    public IReadOnlyList<string> Messages => _messages;
}

/// <summary>
/// Internal helper for extracting validation errors.
/// </summary>
internal static class ValidationHelper
{
    public static List<string> ExtractMessages(Error error, string fieldName)
    {
        if (error.Metadata is null || !error.Metadata.TryGetValue(fieldName, out var rawValue))
        {
            var availableFields = error.Metadata?.Keys.OrderBy(k => k);
            var fieldsString = availableFields != null ? string.Join(", ", availableFields) : "none";
            throw new ArgumentException($"No validation errors found for field '{fieldName}'. Available fields: {fieldsString}");
        }

        return rawValue switch
        {
            null => [],
            string s => [s],
            JsonElement element when element.ValueKind == JsonValueKind.String => [element.GetString() ?? string.Empty],
            JsonElement element when element.ValueKind == JsonValueKind.Array => [.. element.EnumerateArray().Select(e => e.GetString() ?? string.Empty)],
            IEnumerable<string> stringList => stringList.ToList(),
            System.Collections.IEnumerable enumerable => [.. enumerable.Cast<object>().Select(e => e?.ToString() ?? string.Empty)],
            _ => [rawValue.ToString() ?? string.Empty]
        };
    }
}
