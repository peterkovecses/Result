using System.Diagnostics;
using System.Text.Json;
using Xunit;

namespace Kovecses.Result.FluentAssertions;

/// <summary>
/// Provides fluent assertions for validation error messages from a single property.
/// </summary>
public class ValidationPropertyAssertions
{
    private readonly IReadOnlyList<string> _messages;
    private readonly ErrorAssertions? _parent;

    /// <summary>
    /// Initializes a new instance of the <see cref="ValidationPropertyAssertions"/> class.
    /// </summary>
    /// <param name="messages">The validation messages for a property.</param>
    [StackTraceHidden]
    public ValidationPropertyAssertions(IReadOnlyList<string> messages)
    {
        _messages = messages;
        _parent = null;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ValidationPropertyAssertions"/> class with parent chaining support.
    /// </summary>
    /// <param name="messages">The validation messages for a property.</param>
    /// <param name="parent">The parent error assertions for chaining.</param>
    [StackTraceHidden]
    public ValidationPropertyAssertions(IReadOnlyList<string> messages, ErrorAssertions parent)
    {
        _messages = messages;
        _parent = parent;
    }

    /// <summary>
    /// Gets the parent error assertions to allow chaining back to error assertions.
    /// Only available when created with a parent error assertions instance.
    /// </summary>
    public ErrorAssertions And
    {
        get
        {
            if (_parent is null)
            {
                throw new InvalidOperationException("Cannot chain back to error assertions; this ValidationPropertyAssertions was created without a parent.");
            }

            return _parent;
        }
    }

    /// <summary>
    /// Asserts that the validation messages contain the specified message.
    /// </summary>
    /// <param name="expectedMessage">The expected message or part of it.</param>
    /// <returns>The <see cref="ValidationPropertyAssertions"/> for further assertions.</returns>
    public ValidationPropertyAssertions Contain(string expectedMessage)
    {
        Assert.Contains(_messages, m => m.Contains(expectedMessage, StringComparison.OrdinalIgnoreCase));

        return this;
    }

    /// <summary>
    /// Asserts that the validation messages contain all the specified messages.
    /// </summary>
    /// <param name="expectedMessages">The expected messages.</param>
    /// <returns>The <see cref="ValidationPropertyAssertions"/> for further assertions.</returns>
    public ValidationPropertyAssertions ContainAll(params string[] expectedMessages)
    {
        foreach (var message in expectedMessages)
        {
            Contain(message);
        }

        return this;
    }

    /// <summary>
    /// Asserts that the number of validation messages matches the expected count.
    /// </summary>
    /// <param name="expectedCount">The expected message count.</param>
    /// <returns>The <see cref="ValidationPropertyAssertions"/> for further assertions.</returns>
    public ValidationPropertyAssertions HaveCount(int expectedCount)
    {
        Assert.Equal(expectedCount, _messages.Count);

        return this;
    }

    /// <summary>
    /// Asserts that the validation messages are exactly as specified.
    /// </summary>
    /// <param name="expectedMessages">The expected messages.</param>
    /// <returns>The <see cref="ValidationPropertyAssertions"/> for further assertions.</returns>
    public ValidationPropertyAssertions BeExactly(params string[] expectedMessages)
    {
        Assert.Equal(expectedMessages.Length, _messages.Count);
        foreach (var expected in expectedMessages)
        {
            Assert.Contains(expected, _messages);
        }

        return this;
    }
}

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
