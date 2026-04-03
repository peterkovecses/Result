using System.Diagnostics;
using Xunit;

namespace Kovecses.Result.FluentAssertions;

/// <summary>
/// Provides fluent assertions for <see cref="Result"/>.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="ResultAssertions"/> class.
/// </remarks>
/// <param name="subject">The result instance.</param>
[StackTraceHidden]
public class ResultAssertions(Result subject)
{
    /// <summary>
    /// Gets the result instance being asserted.
    /// </summary>
    protected Result Subject { get; } = subject;

    /// <summary>
    /// Asserts that the result is a success.
    /// </summary>
    /// <returns>The <see cref="ResultAssertions"/> for further assertions.</returns>
    public ResultAssertions BeSuccess()
    {
        Assert.True(Subject.IsSuccess, $"Expected result to be successful, but it failed with error: {Subject.Error?.Code}");
        
        return this;
    }

    /// <summary>
    /// Asserts that the result is a failure.
    /// </summary>
    /// <returns>The <see cref="ResultAssertions"/> for further assertions.</returns>
    public ResultAssertions BeFailure()
    {
        Assert.True(Subject.IsFailure, "Expected result to be a failure, but it was successful.");
        
        return this;
    }

    /// <summary>
    /// Asserts that the result has a specific error code.
    /// </summary>
    /// <param name="expectedCode">The expected error code.</param>
    /// <returns>The <see cref="ResultAssertions"/> for further assertions.</returns>
    public ResultAssertions HaveErrorCode(string expectedCode)
    {
        Assert.NotNull(Subject.Error);
        Assert.Equal(expectedCode, Subject.Error!.Code);
        
        return this;
    }

    /// <summary>
    /// Starts assertions on the error of the result.
    /// </summary>
    /// <returns>An <see cref="ErrorAssertions{TAssertions}"/> object.</returns>
    public ErrorAssertions<ResultAssertions> HaveError()
    {
        Assert.NotNull(Subject.Error);
        
        return new ErrorAssertions<ResultAssertions>(Subject.Error, this);
    }

    /// <summary>
    /// Asserts that the result has a validation error for the specified field.
    /// </summary>
    /// <param name="fieldName">The name of the field.</param>
    /// <returns>A <see cref="ValidationAssertions{TAssertions}"/> object for further assertions.</returns>
    public ValidationAssertions<ResultAssertions> HaveValidationErrorFor(string fieldName)
    {
        BeFailure();
        Assert.Equal(ErrorType.Validation, Subject.Error!.Type);
        
        var messages = ValidationHelper.ExtractMessages(Subject.Error!, fieldName);
        Assert.NotEmpty(messages);
        
        return new ValidationAssertions<ResultAssertions>(messages, this);
    }

    /// <summary>
    /// Asserts that the result metadata contains the specified key.
    /// </summary>
    /// <param name="key">The metadata key.</param>
    /// <returns>The <see cref="ResultAssertions"/> for further assertions.</returns>
    public ResultAssertions HaveMetadata(string key)
    {
        Assert.NotNull(Subject.Metadata);
        Assert.Contains(key, Subject.Metadata.Keys);

        return this;
    }

    /// <summary>
    /// Asserts that the result metadata contains the specified key and value.
    /// </summary>
    /// <param name="key">The metadata key.</param>
    /// <param name="expectedValue">The expected metadata value.</param>
    /// <returns>The <see cref="ResultAssertions"/> for further assertions.</returns>
    public ResultAssertions HaveMetadata(string key, object? expectedValue)
    {
        HaveMetadata(key);
        var actualValue = Subject.Metadata![key];

        if (actualValue is System.Text.Json.JsonElement element)
        {
            object? value = element.ValueKind switch
            {
                System.Text.Json.JsonValueKind.String => element.GetString(),
                System.Text.Json.JsonValueKind.Number => element.GetDouble(),
                System.Text.Json.JsonValueKind.True => true,
                System.Text.Json.JsonValueKind.False => false,
                System.Text.Json.JsonValueKind.Null => null,
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

/// <summary>
/// Provides fluent assertions for <see cref="Result{TData}"/>.
/// </summary>
/// <typeparam name="TData">The type of the data.</typeparam>
/// <remarks>
/// Initializes a new instance of the <see cref="ResultAssertions{TData}"/> class.
/// </remarks>
/// <param name="subject">The result instance.</param>
[StackTraceHidden]
public class ResultAssertions<TData>(Result<TData> subject) : ResultAssertions(subject)
{
    private readonly Result<TData> _subject = subject;

    /// <summary>
    /// Asserts that the result is a success.
    /// </summary>
    /// <returns>The <see cref="ResultAssertions{TData}"/> for further assertions.</returns>
    public new ResultAssertions<TData> BeSuccess()
    {
        base.BeSuccess();
        
        return this;
    }

    /// <summary>
    /// Asserts that the result is a failure.
    /// </summary>
    /// <returns>The <see cref="ResultAssertions{TData}"/> for further assertions.</returns>
    public new ResultAssertions<TData> BeFailure()
    {
        base.BeFailure();
        
        return this;
    }

    /// <summary>
    /// Asserts that the result contains specific data.
    /// </summary>
    /// <param name="expectedData">The expected data.</param>
    /// <returns>The <see cref="ResultAssertions{TData}"/> for further assertions.</returns>
    public ResultAssertions<TData> HaveData(TData expectedData)
    {
        BeSuccess();
        Assert.Equal(expectedData, _subject.Data);
        
        return this;
    }

    /// <summary>
    /// Asserts that the result has data matching the predicate.
    /// </summary>
    /// <param name="predicate">The predicate to check the data.</param>
    /// <returns>The <see cref="ResultAssertions{TData}"/> for further assertions.</returns>
    public ResultAssertions<TData> HaveData(Func<TData?, bool> predicate)
    {
        BeSuccess();
        Assert.True(predicate(_subject.Data), "The data does not match the expected predicate.");
        
        return this;
    }

    /// <summary>
    /// Starts assertions on the error of the result.
    /// </summary>
    /// <returns>An <see cref="ErrorAssertions{TAssertions}"/> object.</returns>
    public new ErrorAssertions<ResultAssertions<TData>> HaveError()
    {
        Assert.NotNull(Subject.Error);
        
        return new ErrorAssertions<ResultAssertions<TData>>(Subject.Error, this);
    }

    /// <summary>
    /// Asserts that the result has a validation error for the specified field.
    /// </summary>
    /// <param name="fieldName">The name of the field.</param>
    /// <returns>A <see cref="ValidationAssertions{TAssertions}"/> object for further assertions.</returns>
    public new ValidationAssertions<ResultAssertions<TData>> HaveValidationErrorFor(string fieldName)
    {
        BeFailure();
        Assert.Equal(ErrorType.Validation, Subject.Error!.Type);
        
        var messages = ValidationHelper.ExtractMessages(Subject.Error!, fieldName);
        Assert.NotEmpty(messages);
        
        return new ValidationAssertions<ResultAssertions<TData>>(messages, this);
    }

    /// <summary>
    /// Asserts that the result metadata contains the specified key.
    /// </summary>
    /// <param name="key">The metadata key.</param>
    /// <returns>The <see cref="ResultAssertions{TData}"/> for further assertions.</returns>
    public new ResultAssertions<TData> HaveMetadata(string key)
    {
        base.HaveMetadata(key);

        return this;
    }

    /// <summary>
    /// Asserts that the result metadata contains the specified key and value.
    /// </summary>
    /// <param name="key">The metadata key.</param>
    /// <param name="expectedValue">The expected metadata value.</param>
    /// <returns>The <see cref="ResultAssertions{TData}"/> for further assertions.</returns>
    public new ResultAssertions<TData> HaveMetadata(string key, object? expectedValue)
    {
        base.HaveMetadata(key, expectedValue);

        return this;
    }

    /// <summary>
    /// Asserts that calling ValueOrThrow on the result throws an exception of the specified type.
    /// </summary>
    /// <typeparam name="TException">The type of the expected exception.</typeparam>
    /// <param name="exceptionFactory">An optional factory to create the exception to throw.</param>
    /// <returns>The caught exception for further assertions.</returns>
    public TException ThrowOnValueAccess<TException>(Func<Error, Exception>? exceptionFactory = null) where TException : Exception
    {
        BeFailure();
        
        return Assert.Throws<TException>(() => _subject.ValueOrThrow(exceptionFactory));
    }
}
