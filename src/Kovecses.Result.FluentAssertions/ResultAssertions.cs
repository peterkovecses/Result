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
    public virtual ResultAssertions BeSuccess()
    {
        var errorCodes = Subject.Errors is not null 
            ? string.Join(", ", Subject.Errors.Select(e => e.Code)) 
            : "none";

        Assert.True(Subject.IsSuccess, $"Expected result to be successful, but it failed with errors: {errorCodes}");
        
        return this;
    }

    /// <summary>
    /// Asserts that the result is a failure.
    /// </summary>
    /// <returns>The <see cref="ResultAssertions"/> for further assertions.</returns>
    public virtual ResultAssertions BeFailure()
    {
        Assert.True(Subject.IsFailure, "Expected result to be a failure, but it was successful.");
        
        return this;
    }

    /// <summary>
    /// Starts assertions on the errors of the result.
    /// </summary>
    /// <returns>An <see cref="ErrorCollectionAssertions{TAssertions}"/> object.</returns>
    public ErrorCollectionAssertions<ResultAssertions> HaveErrors()
    {
        Assert.NotNull(Subject.Errors);
        
        return new ErrorCollectionAssertions<ResultAssertions>(Subject.Errors!, this);
    }

    /// <summary>
    /// Asserts that the result contains at least one error with the specified code.
    /// </summary>
    /// <param name="expectedCode">The expected error code.</param>
    /// <returns>The <see cref="ErrorAssertions{TAssertions}"/> for the found error.</returns>
    public ErrorAssertions<ResultAssertions> HaveError(string expectedCode)
    {
        BeFailure();
        var error = Subject.Errors!.FirstOrDefault(e => e.Code == expectedCode);
        
        Assert.NotNull(error);
        
        return new ErrorAssertions<ResultAssertions>(error, this);
    }

    /// <summary>
    /// Asserts that the result has a validation error for the specified field.
    /// </summary>
    /// <param name="fieldName">The name of the field (Error.Code).</param>
    /// <returns>A <see cref="ValidationAssertions{TAssertions}"/> object for further assertions.</returns>
    public ValidationAssertions<ResultAssertions> HaveValidationErrorFor(string fieldName)
    {
        BeFailure();
        
        var messages = Subject.Errors!
            .Where(e => e.Type == ErrorType.Validation && e.Code == fieldName)
            .Select(e => e.Message)
            .ToList();

        Assert.NotEmpty(messages);
        
        return new ValidationAssertions<ResultAssertions>(messages, this);
    }

    /// <summary>
    /// Asserts that the result metadata contains the specified key.
    /// </summary>
    /// <param name="key">The metadata key.</param>
    /// <returns>The <see cref="ResultAssertions"/> for further assertions.</returns>
    public virtual ResultAssertions HaveMetadata(string key)
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
    public virtual ResultAssertions HaveMetadata(string key, object? expectedValue)
    {
        HaveMetadata(key);
        var actualValue = Subject.Metadata![key];

        Assert.Equal(expectedValue, actualValue);

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

    /// <inheritdoc />
    public override ResultAssertions<TData> BeSuccess()
    {
        base.BeSuccess();
        
        return this;
    }

    /// <inheritdoc />
    public override ResultAssertions<TData> BeFailure()
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
    /// Starts assertions on the errors of the result.
    /// </summary>
    /// <returns>An <see cref="ErrorCollectionAssertions{TAssertions}"/> object.</returns>
    public new ErrorCollectionAssertions<ResultAssertions<TData>> HaveErrors()
    {
        Assert.NotNull(Subject.Errors);
        
        return new ErrorCollectionAssertions<ResultAssertions<TData>>(Subject.Errors!, this);
    }

    /// <summary>
    /// Asserts that the result contains at least one error with the specified code.
    /// </summary>
    /// <param name="expectedCode">The expected error code.</param>
    /// <returns>The <see cref="ErrorAssertions{TAssertions}"/> for the found error.</returns>
    public new ErrorAssertions<ResultAssertions<TData>> HaveError(string expectedCode)
    {
        BeFailure();
        var error = Subject.Errors!.FirstOrDefault(e => e.Code == expectedCode);
        
        Assert.NotNull(error);
        
        return new ErrorAssertions<ResultAssertions<TData>>(error, this);
    }

    /// <summary>
    /// Asserts that the result has a validation error for the specified field.
    /// </summary>
    /// <param name="fieldName">The name of the field (Error.Code).</param>
    /// <returns>A <see cref="ValidationAssertions{TAssertions}"/> object for further assertions.</returns>
    public new ValidationAssertions<ResultAssertions<TData>> HaveValidationErrorFor(string fieldName)
    {
        BeFailure();
        
        var messages = Subject.Errors!
            .Where(e => e.Type == ErrorType.Validation && e.Code == fieldName)
            .Select(e => e.Message)
            .ToList();

        Assert.NotEmpty(messages);
        
        return new ValidationAssertions<ResultAssertions<TData>>(messages, this);
    }

    /// <inheritdoc />
    public override ResultAssertions<TData> HaveMetadata(string key)
    {
        base.HaveMetadata(key);

        return this;
    }

    /// <inheritdoc />
    public override ResultAssertions<TData> HaveMetadata(string key, object? expectedValue)
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
    public TException ThrowOnValueAccess<TException>(Func<Error[], Exception>? exceptionFactory = null) where TException : Exception
    {
        BeFailure();
        
        return Assert.Throws<TException>(() => _subject.ValueOrThrow(exceptionFactory));
    }
}
