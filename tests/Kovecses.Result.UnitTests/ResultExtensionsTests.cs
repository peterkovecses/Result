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
}
