using System.Diagnostics;
using System.Text.Json;
using Xunit;

namespace Kovecses.Result.FluentAssertions;

/// <summary>
/// Provides fluent assertions for validation errors.
/// </summary>
/// <typeparam name="TParent">The type of the parent assertions.</typeparam>
/// <param name="subject">The collection of validation messages.</param>
/// <param name="parent">The parent assertions instance.</param>
[StackTraceHidden]
public class ValidationAssertions<TParent>(IReadOnlyList<string> subject, TParent parent) where TParent : ResultAssertions
{
    private readonly IReadOnlyList<string> _subject = subject;

    /// <summary>
    /// Gets the parent assertions to allow chaining.
    /// </summary>
    public TParent And { get; } = parent;

    /// <summary>
    /// Asserts that the validation errors contain the specified message.
    /// </summary>
    /// <param name="expectedMessage">The expected message or part of it.</param>
    /// <returns>The <see cref="ValidationAssertions{TParent}"/> for further assertions.</returns>
    public ValidationAssertions<TParent> Contain(string expectedMessage)
    {
        Assert.Contains(_subject, m => m.Contains(expectedMessage, StringComparison.OrdinalIgnoreCase));
        
        return this;
    }

    /// <summary>
    /// Asserts that the validation errors contain all the specified messages.
    /// </summary>
    /// <param name="expectedMessages">The expected messages.</param>
    /// <returns>The <see cref="ValidationAssertions{TParent}"/> for further assertions.</returns>
    public ValidationAssertions<TParent> ContainAll(params string[] expectedMessages)
    {
        foreach (var message in expectedMessages)
        {
            Contain(message);
        }
        
        return this;
    }

    /// <summary>
    /// Extracts and normalizes validation messages for a specific field from an <see cref="Error"/> object.
    /// This helper is useful for writing custom assertions.
    /// </summary>
    /// <param name="error">The error object containing metadata or direct messages.</param>
    /// <param name="fieldName">The name of the field (the Error.Code or a key in the metadata dictionary).</param>
    /// <returns>A list of normalized error messages.</returns>
    public static List<string> ExtractMessages(Error error, string fieldName)
    {
        var messages = new List<string>();

        // 1. Check if the error itself is for this field (New flat structure)
        if (error.Code == fieldName)
        {
            messages.Add(error.Message);
        }

        // 2. Check metadata for legacy or nested messages
        if (error.Metadata is not null && error.Metadata.TryGetValue(fieldName, out var rawValue))
        {
            messages.AddRange(ExtractFromValue(rawValue));
        }

        if (messages.Count == 0)
        {
            var availableFields = error.Metadata?.Keys.ToList() ?? [];
            if (error.Code != ErrorCodes.Validation)
            {
                availableFields.Add(error.Code);
            }

            var fieldsString = availableFields.Count > 0 
                ? string.Join(", ", availableFields.OrderBy(k => k)) 
                : "none";

            throw new ArgumentException($"No validation errors found for field '{fieldName}'. Available fields: {fieldsString}");
        }

        return messages;
    }

    private static List<string> ExtractFromValue(object? value) => value switch
    {
        null => [],
        string s => [s],
        JsonElement element => ExtractFromJsonElement(element),
        IEnumerable<string> stringList => stringList.ToList(),
        System.Collections.IEnumerable enumerable => [.. enumerable.Cast<object>().Select(e => e?.ToString() ?? string.Empty)],
        _ => [value.ToString() ?? string.Empty]
    };

    private static List<string> ExtractFromJsonElement(JsonElement element) => element.ValueKind switch
    {
        JsonValueKind.String => [element.GetString() ?? string.Empty],
        JsonValueKind.Array => [.. element.EnumerateArray().Select(e => e.GetString() ?? string.Empty)],
        _ => [element.GetRawText()]
    };
}
