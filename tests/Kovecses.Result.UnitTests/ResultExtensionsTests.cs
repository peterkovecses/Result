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
        result.Should().BeFailure().HaveErrorCode(ErrorCodes.NotFound);
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
        var output = await task.MatchAsync(data => data, error => "Failure");

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
            error => Task.FromResult("Failure"));

        // Assert
        Assert.Equal("Data", output);
    }

    [Fact]
    public async Task MatchAsync_WithTaskResultGeneric_WhenFailure_ShouldExecuteOnFailure()
    {
        // Arrange
        var task = Task.FromResult(Result.Failure<string>(Error.NotFound()));

        // Act
        var output = await task.MatchAsync(data => data, error => "Failure");

        // Assert
        Assert.Equal("Failure", output);
    }
}
