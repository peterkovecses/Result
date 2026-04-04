using System.Diagnostics;
using Xunit;

namespace Kovecses.Result.FluentAssertions;

/// <summary>
/// Provides fluent assertions for a collection of <see cref="Error"/>.
/// </summary>
/// <typeparam name="TParent">The type of the parent assertions.</typeparam>
/// <param name="subject">The collection of errors.</param>
/// <param name="parent">The parent assertions instance.</param>
[StackTraceHidden]
public class ErrorCollectionAssertions<TParent>(Error[] subject, TParent parent) where TParent : ResultAssertions
{
    private readonly Error[] _subject = subject;

    /// <summary>
    /// Gets the parent assertions to allow chaining.
    /// </summary>
    public TParent And { get; } = parent;

    /// <summary>
    /// Asserts that the collection contains the specified number of errors.
    /// </summary>
    /// <param name="expectedCount">The expected number of errors.</param>
    /// <returns>The <see cref="ErrorCollectionAssertions{TParent}"/> for further assertions.</returns>
    public ErrorCollectionAssertions<TParent> HaveCount(int expectedCount)
    {
        Assert.Equal(expectedCount, _subject.Length);
        
        return this;
    }

    /// <summary>
    /// Asserts that all errors in the collection are of the specified type.
    /// </summary>
    /// <param name="expectedType">The expected error type.</param>
    /// <returns>The <see cref="ErrorCollectionAssertions{TParent}"/> for further assertions.</returns>
    public ErrorCollectionAssertions<TParent> AllBeOfType(ErrorType expectedType)
    {
        Assert.All(_subject, e => Assert.Equal(expectedType, e.Type));
        
        return this;
    }

    /// <summary>
    /// Asserts that the collection contains an error matching the predicate.
    /// </summary>
    /// <param name="predicate">The predicate to match.</param>
    /// <returns>The <see cref="ErrorCollectionAssertions{TParent}"/> for further assertions.</returns>
    public ErrorCollectionAssertions<TParent> Contain(Func<Error, bool> predicate)
    {
        Assert.Contains(_subject, e => predicate(e));
        
        return this;
    }
}
