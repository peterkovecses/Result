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
    /// <summary>
    /// Gets the error instance being asserted.
    /// </summary>
    protected Error? Subject { get; } = subject;

    /// <summary>
    /// Asserts that the error is not null.
    /// </summary>
    /// <returns>The <see cref="ErrorAssertions"/> for further assertions.</returns>
    public virtual ErrorAssertions NotBeNull()
    {
        Assert.NotNull(Subject);
        
        return this;
    }

    /// <summary>
    /// Asserts that the error code matches the expected code.
    /// </summary>
    /// <param name="expectedCode">The expected error code.</param>
    /// <returns>The <see cref="ErrorAssertions"/> for further assertions.</returns>
    public virtual ErrorAssertions HaveCode(string expectedCode)
    {
        NotBeNull();
        Assert.Equal(expectedCode, Subject!.Code);
        
        return this;
    }

    /// <summary>
    /// Asserts that the error message matches the expected message.
    /// </summary>
    /// <param name="expectedMessage">The expected error message.</param>
    /// <returns>The <see cref="ErrorAssertions"/> for further assertions.</returns>
    public virtual ErrorAssertions HaveMessage(string expectedMessage)
    {
        NotBeNull();
        Assert.Equal(expectedMessage, Subject!.Message);
        
        return this;
    }

    /// <summary>
    /// Asserts that the error type matches the expected type.
    /// </summary>
    /// <param name="expectedType">The expected error type.</param>
    /// <returns>The <see cref="ErrorAssertions"/> for further assertions.</returns>
    public virtual ErrorAssertions HaveType(ErrorType expectedType)
    {
        NotBeNull();
        Assert.Equal(expectedType, Subject!.Type);
        
        return this;
    }

    /// <summary>
    /// Asserts that the error metadata contains the specified key.
    /// </summary>
    /// <param name="key">The metadata key.</param>
    /// <returns>The <see cref="ErrorAssertions"/> for further assertions.</returns>
    public virtual ErrorAssertions HaveMetadata(string key)
    {
        NotBeNull();
        Assert.NotNull(Subject!.Metadata);
        Assert.Contains(key, Subject.Metadata.Keys);
        
        return this;
    }

    /// <summary>
    /// Asserts that the error metadata contains the specified key and value.
    /// </summary>
    /// <param name="key">The metadata key.</param>
    /// <param name="expectedValue">The expected metadata value.</param>
    /// <returns>The <see cref="ErrorAssertions"/> for further assertions.</returns>
    public virtual ErrorAssertions HaveMetadata(string key, object? expectedValue)
    {
        HaveMetadata(key);
        var actualValue = Subject!.Metadata![key];

        MetadataHelper.AssertMetadataValue(expectedValue, actualValue);
        
        return this;
    }

    /// <summary>
    /// Asserts that the error contains validation messages for the specified property name.
    /// The property name must be a key in the metadata with an array of message strings as the value.
    /// Supports both in-memory collections and JSON-deserialized elements.
    /// </summary>
    /// <param name="propertyName">The property name to check for validation messages.</param>
    /// <returns>The <see cref="ValidationPropertyAssertions"/> for further assertions on the messages.</returns>
    public virtual ValidationPropertyAssertions HaveValidationProperty(string propertyName)
    {
        NotBeNull();
        Assert.NotNull(Subject!.Metadata);
        Assert.Contains(propertyName, Subject.Metadata.Keys);

        var value = Subject.Metadata[propertyName];

        // Handle string[]
        if (value is string[] messageArray)
        {
            Assert.NotEmpty(messageArray);

            return new ValidationPropertyAssertions(messageArray.ToList());
        }

        // Handle List<string>
        if (value is List<string> messageList)
        {
            Assert.NotEmpty(messageList);

            return new ValidationPropertyAssertions(messageList);
        }

        // Handle List<object> (from JSON deserialization)
        if (value is List<object> objectList)
        {
            var messages = objectList.OfType<string>().ToList();
            Assert.NotEmpty(messages);

            return new ValidationPropertyAssertions(messages);
        }

        // Handle IEnumerable<string>
        if (value is IEnumerable<string> enumerable)
        {
            var messages = enumerable.ToList();
            Assert.NotEmpty(messages);

            return new ValidationPropertyAssertions(messages);
        }

        // Handle JsonElement (from JSON deserialization)
        if (value is JsonElement jsonElement)
        {
            var messages = ExtractMessagesFromJsonElement(jsonElement);
            Assert.NotEmpty(messages);

            return new ValidationPropertyAssertions(messages);
        }

        throw new InvalidOperationException($"Expected validation messages for property '{propertyName}' to be a string array or list, but got {value?.GetType().Name ?? "null"}");
    }

    /// <summary>
    /// Extracts string messages from a JsonElement array or single value.
    /// </summary>
    protected static List<string> ExtractMessagesFromJsonElement(JsonElement element)
    {
        if (element.ValueKind == JsonValueKind.Array)
        {
            return [.. element.EnumerateArray().Select(e => e.GetString() ?? string.Empty)];
        }

        return [element.GetString() ?? string.Empty];
    }
}

/// <summary>
/// Provides fluent assertions for <see cref="Error"/> with chaining support.
/// </summary>
/// <typeparam name="TParent">The type of the parent assertions.</typeparam>
/// <remarks>
/// Initializes a new instance of the <see cref="ErrorAssertions{TParent}"/> class.
/// </remarks>
/// <param name="subject">The error instance.</param>
/// <param name="parent">The parent assertions instance.</param>
[StackTraceHidden]
public class ErrorAssertions<TParent>(Error? subject, TParent parent) : ErrorAssertions(subject) where TParent : ResultAssertions
{
    /// <summary>
    /// Gets the parent assertions to allow chaining.
    /// </summary>
    public TParent And { get; } = parent;

    /// <inheritdoc />
    public override ErrorAssertions<TParent> NotBeNull()
    {
        base.NotBeNull();

        return this;
    }

    /// <inheritdoc />
    public override ErrorAssertions<TParent> HaveCode(string expectedCode)
    {
        base.HaveCode(expectedCode);

        return this;
    }

    /// <inheritdoc />
    public override ErrorAssertions<TParent> HaveMessage(string expectedMessage)
    {
        base.HaveMessage(expectedMessage);

        return this;
    }

    /// <inheritdoc />
    public override ErrorAssertions<TParent> HaveType(ErrorType expectedType)
    {
        base.HaveType(expectedType);

        return this;
    }

    /// <inheritdoc />
    public override ErrorAssertions<TParent> HaveMetadata(string key)
    {
        base.HaveMetadata(key);

        return this;
    }

    /// <inheritdoc />
    public override ErrorAssertions<TParent> HaveMetadata(string key, object? expectedValue)
    {
        base.HaveMetadata(key, expectedValue);
        
        return this;
    }

    /// <inheritdoc />
    public override ValidationPropertyAssertions HaveValidationProperty(string propertyName)
    {
        NotBeNull();
        Assert.NotNull(Subject!.Metadata);
        Assert.Contains(propertyName, Subject.Metadata.Keys);

        var value = Subject.Metadata[propertyName];

        // Handle string[]
        if (value is string[] messageArray)
        {
            Assert.NotEmpty(messageArray);

            return new ValidationPropertyAssertions(messageArray.ToList(), this);
        }

        // Handle List<string>
        if (value is List<string> messageList)
        {
            Assert.NotEmpty(messageList);

            return new ValidationPropertyAssertions(messageList, this);
        }

        // Handle List<object> (from JSON deserialization)
        if (value is List<object> objectList)
        {
            var messages = objectList.OfType<string>().ToList();
            Assert.NotEmpty(messages);

            return new ValidationPropertyAssertions(messages, this);
        }

        // Handle IEnumerable<string>
        if (value is IEnumerable<string> enumerable)
        {
            var messages = enumerable.ToList();
            Assert.NotEmpty(messages);

            return new ValidationPropertyAssertions(messages, this);
        }

        // Handle JsonElement (from JSON deserialization)
        if (value is JsonElement jsonElement)
        {
            var messages = ExtractMessagesFromJsonElement(jsonElement);
            Assert.NotEmpty(messages);

            return new ValidationPropertyAssertions(messages, this);
        }

        throw new InvalidOperationException($"Expected validation messages for property '{propertyName}' to be a string array or list, but got {value?.GetType().Name ?? "null"}");
    }
}
