using System.Diagnostics;
using System.Text.Json;
using Xunit;

namespace Kovecses.Result.FluentAssertions;

/// <summary>
/// Provides fluent assertions for <see cref="Error"/>.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="ErrorAssertions"/> class.
/// </remarks>
/// <param name="subject">The error instance.</param>
[StackTraceHidden]
public class ErrorAssertions(Error? subject)
{
    private readonly Error? _subject = subject;

    /// <summary>
    /// Asserts that the error is not null.
    /// </summary>
    /// <returns>The <see cref="ErrorAssertions"/> for further assertions.</returns>
    public ErrorAssertions NotBeNull()
    {
        Assert.NotNull(_subject);

        return this;
    }

    /// <summary>
    /// Asserts that the error code matches the expected code.
    /// </summary>
    /// <param name="expectedCode">The expected error code.</param>
    /// <returns>The <see cref="ErrorAssertions"/> for further assertions.</returns>
    public ErrorAssertions HaveCode(string expectedCode)
    {
        NotBeNull();
        Assert.Equal(expectedCode, _subject!.Code);

        return this;
    }

    /// <summary>
    /// Asserts that the error message matches the expected message.
    /// </summary>
    /// <param name="expectedMessage">The expected error message.</param>
    /// <returns>The <see cref="ErrorAssertions"/> for further assertions.</returns>
    public ErrorAssertions HaveMessage(string expectedMessage)
    {
        NotBeNull();
        Assert.Equal(expectedMessage, _subject!.Message);

        return this;
    }

    /// <summary>
    /// Asserts that the error type matches the expected type.
    /// </summary>
    /// <param name="expectedType">The expected error type.</param>
    /// <returns>The <see cref="ErrorAssertions"/> for further assertions.</returns>
    public ErrorAssertions HaveType(ErrorType expectedType)
    {
        NotBeNull();
        Assert.Equal(expectedType, _subject!.Type);

        return this;
    }

    /// <summary>
    /// Asserts that the error metadata contains the specified key.
    /// </summary>
    /// <param name="key">The metadata key.</param>
    /// <returns>The <see cref="ErrorAssertions"/> for further assertions.</returns>
    public ErrorAssertions HaveMetadata(string key)
    {
        NotBeNull();
        Assert.NotNull(_subject!.Metadata);
        Assert.Contains(key, _subject.Metadata.Keys);
        
        return this;
    }

    /// <summary>
    /// Asserts that the error metadata contains the specified key and value.
    /// </summary>
    /// <param name="key">The metadata key.</param>
    /// <param name="expectedValue">The expected metadata value.</param>
    /// <returns>The <see cref="ErrorAssertions"/> for further assertions.</returns>
    public ErrorAssertions HaveMetadata(string key, object? expectedValue)
    {
        HaveMetadata(key);
        var actualValue = _subject!.Metadata![key];

        if (actualValue is JsonElement element)
        {
            object? value = element.ValueKind switch
            {
                JsonValueKind.String => element.GetString(),
                JsonValueKind.Number => element.GetDouble(),
                JsonValueKind.True => true,
                JsonValueKind.False => false,
                JsonValueKind.Null => null,
                _ => element.GetRawText()
            };
            
            Assert.Equal(expectedValue?.ToString(), value?.ToString());
        }
        else
        {
            Assert.Equal(expectedValue, actualValue);
        }

        return this;
    }
}
