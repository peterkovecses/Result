using Kovecses.Result.FluentAssertions;
using Xunit;

namespace Kovecses.Result.UnitTests;

public class ResultExtensionsTests
{
    [Fact]
    public async Task BindAsync_WithTaskResult_WhenSuccess_ShouldChain()
    {
        // Arrange
        var task = Task.FromResult(Result.Success());

        // Act
        var result = await task.BindAsync(() => Task.FromResult(Result.Success()));

        // Assert
        result.Should().BeSuccess();
    }

    [Fact]
    public async Task BindAsync_WithTaskResult_WhenFailure_ShouldReturnFailure()
    {
        // Arrange
        var task = Task.FromResult(Result.Failure(Error.NotFound()));

        // Act
        var result = await task.BindAsync(() => Task.FromResult(Result.Success()));

        // Assert
        result.Should().BeFailure().HaveError(ErrorCodes.NotFound);
    }

    [Fact]
    public async Task MapAsync_WithTaskResultGeneric_WhenSuccess_ShouldTransform()
    {
        // Arrange
        var task = Task.FromResult(Result.Success(10));

        // Act
        var result = await task.MapAsync(x => x * 2);

        // Assert
        result.Should().BeSuccess().HaveData(20);
    }

    [Fact]
    public async Task MapAsync_WithTaskResultGeneric_TaskMapping_WhenSuccess_ShouldTransform()
    {
        // Arrange
        var task = Task.FromResult(Result.Success(10));

        // Act
        var result = await task.MapAsync(x => Task.FromResult(x * 2));

        // Assert
        result.Should().BeSuccess().HaveData(20);
    }

    [Fact]
    public async Task BindAsync_WithTaskResultGeneric_WhenSuccess_ShouldChain()
    {
        // Arrange
        var task = Task.FromResult(Result.Success(10));

        // Act
        var result = await task.BindAsync(x => Task.FromResult(Result.Success(x.ToString())));

        // Assert
        result.Should().BeSuccess().HaveData("10");
    }

    [Fact]
    public async Task MatchAsync_WithTaskResultGeneric_WhenSuccess_ShouldExecuteOnSuccess()
    {
        // Arrange
        var task = Task.FromResult(Result.Success("Data"));

        // Act
        var output = await task.MatchAsync(data => data, errors => "Failure");

        // Assert
        Assert.Equal("Data", output);
    }

    [Fact]
    public async Task MatchAsync_WithTaskResultGeneric_AsyncFunctions_WhenSuccess_ShouldExecuteOnSuccess()
    {
        // Arrange
        var task = Task.FromResult(Result.Success("Data"));

        // Act
        var output = await task.MatchAsync(
            data => Task.FromResult(data), 
            errors => Task.FromResult("Failure"));

        // Assert
        Assert.Equal("Data", output);
    }

    [Fact]
    public async Task MatchAsync_WithTaskResultGeneric_WhenFailure_ShouldExecuteOnFailure()
    {
        // Arrange
        var task = Task.FromResult(Result.Failure<string>(Error.NotFound()));

        // Act
        var output = await task.MatchAsync(data => data, errors => "Failure");

        // Assert
        Assert.Equal("Failure", output);
    }

    [Fact]
    public async Task MatchAsync_WithTaskResultGeneric_AsyncFunctions_WhenFailure_ShouldExecuteOnFailure()
    {
        // Arrange
        var task = Task.FromResult(Result.Failure<string>(Error.NotFound()));

        // Act
        var output = await task.MatchAsync(
            data => Task.FromResult(data), 
            errors => Task.FromResult("Failure"));

        // Assert
        Assert.Equal("Failure", output);
    }

    [Fact]
    public async Task TapAsync_WithTaskResult_WhenSuccess_ShouldExecuteAction()
    {
        // Arrange
        var task = Task.FromResult(Result.Success());
        var executed = false;

        // Act
        var result = await task.TapAsync(() => executed = true);

        // Assert
        Assert.True(executed);
        result.Should().BeSuccess();
    }

    [Fact]
    public async Task TapAsync_WithTaskResultGeneric_WhenSuccess_ShouldExecuteActionWithData()
    {
        // Arrange
        var task = Task.FromResult(Result.Success(10));
        var value = 0;

        // Act
        var result = await task.TapAsync(data => value = data);

        // Assert
        Assert.Equal(10, value);
        result.Should().BeSuccess().HaveData(10);
    }

    [Fact]
    public async Task TapAsync_WithTaskResultGeneric_TaskFunc_WhenSuccess_ShouldExecuteFuncWithData()
    {
        // Arrange
        var task = Task.FromResult(Result.Success(10));
        var value = 0;

        // Act
        var result = await task.TapAsync(data => { value = data; return Task.CompletedTask; });

        // Assert
        Assert.Equal(10, value);
        result.Should().BeSuccess().HaveData(10);
    }

    [Fact]
    public async Task BindAsync_Extension_WhenFailure_ShouldNotExecuteNext()
    {
        // Arrange
        var task = Task.FromResult(Result.Failure(Error.NotFound()));
        var executed = false;

        // Act
        var result = await task.BindAsync(() => { executed = true; return Task.FromResult(Result.Success()); });

        // Assert
        Assert.False(executed);
        result.Should().BeFailure();
    }

    [Fact]
    public async Task MapAsync_Extension_WhenFailure_ShouldReturnFailure()
    {
        // Arrange
        var task = Task.FromResult(Result.Failure<int>(Error.NotFound()));

        // Act
        var result = await task.MapAsync(x => x * 2);

        // Assert
        result.Should().BeFailure();
    }

    [Fact]
    public async Task MapAsync_Extension_TaskMapping_WhenFailure_ShouldReturnFailure()
    {
        // Arrange
        var task = Task.FromResult(Result.Failure<int>(Error.NotFound()));

        // Act
        var result = await task.MapAsync(x => Task.FromResult(x * 2));

        // Assert
        result.Should().BeFailure();
    }

    [Fact]
    public async Task BindAsync_ExtensionGeneric_WhenFailure_ShouldReturnFailure()
    {
        // Arrange
        var task = Task.FromResult(Result.Failure<int>(Error.NotFound()));

        // Act
        var result = await task.BindAsync(x => Task.FromResult(Result.Success(x.ToString())));

        // Assert
        result.Should().BeFailure();
    }

    [Fact]
    public async Task TapAsync_Extension_WhenFailure_ShouldNotExecuteAction()
    {
        // Arrange
        var task = Task.FromResult(Result.Failure(Error.NotFound()));
        var executed = false;

        // Act
        var result = await task.TapAsync(() => executed = true);

        // Assert
        Assert.False(executed);
        result.Should().BeFailure();
    }

    [Fact]
    public async Task TapAsync_Extension_TaskFunc_WhenFailure_ShouldNotExecuteFunc()
    {
        // Arrange
        var task = Task.FromResult(Result.Failure(Error.NotFound()));
        var executed = false;

        // Act
        var result = await task.TapAsync(() => { executed = true; return Task.CompletedTask; });

        // Assert
        Assert.False(executed);
        result.Should().BeFailure();
    }

    [Fact]
    public async Task TapAsync_ExtensionGeneric_WhenFailure_ShouldNotExecuteAction()
    {
        // Arrange
        var task = Task.FromResult(Result.Failure<int>(Error.NotFound()));
        var executed = false;

        // Act
        var result = await task.TapAsync(_ => executed = true);

        // Assert
        Assert.False(executed);
        result.Should().BeFailure();
    }

    [Fact]
    public async Task TapAsync_ExtensionGeneric_TaskFunc_WhenFailure_ShouldNotExecuteFunc()
    {
        // Arrange
        var task = Task.FromResult(Result.Failure<int>(Error.NotFound()));
        var executed = false;

        // Act
        var result = await task.TapAsync(_ => { executed = true; return Task.CompletedTask; });

        // Assert
        Assert.False(executed);
        result.Should().BeFailure();
    }

    [Fact]
    public void JoinErrorMessages_WhenFailure_ShouldJoinMessagesWithSeparator()
    {
        // Arrange
        var errors = new[]
        {
            Error.Validation("V.1", "Message 1"),
            Error.Validation("V.2", "Message 2")
        };
        var result = Result.Failure(errors);

        // Act
        var message = result.JoinErrorMessages(" | ");

        // Assert
        Assert.Equal("Message 1 | Message 2", message);
    }

    [Fact]
    public void JoinErrorMessages_WhenSuccess_ShouldReturnEmptyString()
    {
        // Act
        var result = Result.Success();

        // Assert
        Assert.Empty(result.JoinErrorMessages());
    }

    [Fact]
    public async Task BindAsync_WithSynchronousGenericCallback_WhenSuccessful_ShouldChain()
    {
        var result = await Task.FromResult(Result.Success(10))
            .BindAsync(value => Result.Success(value.ToString()));

        result.Should().BeSuccess().HaveData("10");
    }

    [Fact]
    public async Task BindAsync_WithSynchronousGenericCallback_WhenFailed_ShouldCarryFailure()
    {
        var result = await Task.FromResult(Result.Failure<int>(Error.NotFound()))
            .BindAsync(value => Result.Success(value.ToString()));

        result.Should().BeFailure().HaveError(ErrorCodes.NotFound);
    }

    [Theory]
    [InlineData(true, "Success")]
    [InlineData(false, "Failure")]
    public async Task MatchAsync_WithTaskResult_WhenUsingSynchronousCallbacks_ShouldMatch(bool succeeds, string expected)
    {
        Task<Result> task = succeeds
            ? Task.FromResult(Result.Success())
            : Task.FromResult(Result.Failure(Error.NotFound()));

        var actual = await task.MatchAsync(() => "Success", _ => "Failure");

        Assert.Equal(expected, actual);
    }

    [Theory]
    [InlineData(true, "Success")]
    [InlineData(false, "Failure")]
    public async Task MatchAsync_WithTaskResult_WhenUsingAsynchronousCallbacks_ShouldMatch(bool succeeds, string expected)
    {
        Task<Result> task = succeeds
            ? Task.FromResult(Result.Success())
            : Task.FromResult(Result.Failure(Error.NotFound()));

        var actual = await task.MatchAsync(
            () => Task.FromResult("Success"),
            _ => Task.FromResult("Failure"));

        Assert.Equal(expected, actual);
    }

    [Fact]
    public void ToResult_WhenConvertingToGenericResult_ShouldPreserveStateAndMetadata()
    {
        var metadata = new Dictionary<string, object> { ["source"] = "catalog" };
        var success = Result.Success(metadata).ToResult("book-1");
        var failure = Result.Failure(Error.NotFound(), metadata).ToResult("book-1");

        success.Should().BeSuccess().HaveData("book-1").HaveMetadata("source", "catalog");
        failure.Should().BeFailure().HaveError(ErrorCodes.NotFound);
        Assert.Equal("catalog", failure.Metadata!["source"]);
    }

    [Fact]
    public void ToResult_WhenDiscardingGenericResultData_ShouldPreserveStateAndMetadata()
    {
        var metadata = new Dictionary<string, object> { ["source"] = "catalog" };
        var success = Result.Success("book-1", metadata).ToResult();
        var failure = Result.Failure<string>(Error.NotFound(), metadata).ToResult();

        success.Should().BeSuccess().HaveMetadata("source", "catalog");
        failure.Should().BeFailure().HaveError(ErrorCodes.NotFound);
        Assert.Equal("catalog", failure.Metadata!["source"]);
    }

    [Fact]
    public void Ensure_WhenPredicatePasses_ShouldReturnOriginalResult()
    {
        var result = Result.Success("book-1");

        var ensured = result.Ensure(value => value.StartsWith("book-", StringComparison.Ordinal), Error.Failure());

        Assert.Same(result, ensured);
    }

    [Fact]
    public void Ensure_WhenPredicateFails_ShouldReturnFailure()
    {
        var result = Result.Success("invalid");

        var ensured = result.Ensure(value => value.StartsWith("book-", StringComparison.Ordinal), Error.Validation("Book.Isbn", "Invalid ISBN."));

        ensured.Should().BeFailure().HaveError("Book.Isbn");
    }

    [Fact]
    public async Task EnsureAsync_WithSynchronousPredicate_ShouldPreserveOrFail()
    {
        var success = await Task.FromResult(Result.Success("book-1"))
            .EnsureAsync(value => value.StartsWith("book-", StringComparison.Ordinal), Error.Failure());
        var failure = await Task.FromResult(Result.Success("invalid"))
            .EnsureAsync(value => value.StartsWith("book-", StringComparison.Ordinal), Error.Validation("Book.Isbn", "Invalid ISBN."));

        success.Should().BeSuccess().HaveData("book-1");
        failure.Should().BeFailure().HaveError("Book.Isbn");
    }

    [Fact]
    public async Task EnsureAsync_WithAsynchronousPredicate_ShouldPreserveOrFail()
    {
        var success = await Task.FromResult(Result.Success("book-1"))
            .EnsureAsync(value => Task.FromResult(value.StartsWith("book-", StringComparison.Ordinal)), Error.Failure());
        var failure = await Task.FromResult(Result.Success("invalid"))
            .EnsureAsync(value => Task.FromResult(value.StartsWith("book-", StringComparison.Ordinal)), Error.Validation("Book.Isbn", "Invalid ISBN."));

        success.Should().BeSuccess().HaveData("book-1");
        failure.Should().BeFailure().HaveError("Book.Isbn");
    }

    [Fact]
    public void Ensure_WhenFailure_ShouldSkipPredicateAndPreserveErrorsAndMetadata()
    {
        // Arrange
        var errors = new[] { Error.NotFound(), Error.Failure("Book.Unavailable", "Book is unavailable.") };
        var metadata = new Dictionary<string, object> { ["source"] = "catalog" };
        var result = Result.Failure<string>(errors, metadata);
        var predicateExecuted = false;

        // Act
        var ensured = result.Ensure(_ =>
        {
            predicateExecuted = true;
            return true;
        }, Error.Failure());

        // Assert
        Assert.False(predicateExecuted);
        Assert.Equal(errors, ensured.Errors);
        Assert.Equal("catalog", ensured.Metadata!["source"]);
    }

    [Fact]
    public async Task EnsureAsync_WithSynchronousPredicate_WhenFailure_ShouldSkipPredicateAndPreserveErrorsAndMetadata()
    {
        // Arrange
        var errors = new[] { Error.NotFound(), Error.Failure("Book.Unavailable", "Book is unavailable.") };
        var metadata = new Dictionary<string, object> { ["source"] = "catalog" };
        var result = Result.Failure<string>(errors, metadata);
        var predicateExecuted = false;

        // Act
        var ensured = await Task.FromResult(result).EnsureAsync(_ =>
        {
            predicateExecuted = true;
            return true;
        }, Error.Failure());

        // Assert
        Assert.False(predicateExecuted);
        Assert.Equal(errors, ensured.Errors);
        Assert.Equal("catalog", ensured.Metadata!["source"]);
    }

    [Fact]
    public async Task EnsureAsync_WithAsynchronousPredicate_WhenFailure_ShouldSkipPredicateAndPreserveErrorsAndMetadata()
    {
        // Arrange
        var errors = new[] { Error.NotFound(), Error.Failure("Book.Unavailable", "Book is unavailable.") };
        var metadata = new Dictionary<string, object> { ["source"] = "catalog" };
        var result = Result.Failure<string>(errors, metadata);
        var predicateExecuted = false;

        // Act
        var ensured = await Task.FromResult(result).EnsureAsync(_ =>
        {
            predicateExecuted = true;
            return Task.FromResult(true);
        }, Error.Failure());

        // Assert
        Assert.False(predicateExecuted);
        Assert.Equal(errors, ensured.Errors);
        Assert.Equal("catalog", ensured.Metadata!["source"]);
    }
}
