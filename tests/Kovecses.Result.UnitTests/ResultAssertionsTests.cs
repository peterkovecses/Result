namespace Kovecses.Result.UnitTests;

public class ResultAssertionsTests
{
    [Fact]
    public void HaveData_WithAction_WhenSuccessAndDataNotNull_ShouldExecuteInspector()
    {
        // Arrange
        var data = new { Name = "Test", Value = 123 };
        var result = Result.Success(data);
        var executed = false;

        // Act & Assert
        result.Should().HaveData(d =>
        {
            Assert.Equal("Test", d.Name);
            Assert.Equal(123, d.Value);
            executed = true;
        });

        Assert.True(executed);
    }

    [Fact]
    public void WhichData_WhenSuccessAndDataNotNull_ShouldReturnData()
    {
        // Arrange
        var data = new { Name = "Test", Value = 123 };
        var result = Result.Success(data);

        // Act
        var actualData = result.Should().WhichData;

        // Assert
        Assert.Equal("Test", actualData.Name);
        Assert.Equal(123, actualData.Value);
    }

    [Fact]
    public void HaveData_WithAction_WhenFailure_ShouldThrow()
    {
        // Arrange
        var result = Result.Failure<string>(Error.Failure("Code", "Message"));

        // Act
        var act = () => result.Should().HaveData(d => { });

        // Assert
        Assert.ThrowsAny<Exception>(act);
    }

    [Fact]
    public void WhichData_WhenFailure_ShouldThrow()
    {
        // Arrange
        var result = Result.Failure<string>(Error.Failure("Code", "Message"));

        // Act
        var act = () => _ = result.Should().WhichData;

        // Assert
        Assert.ThrowsAny<Exception>(act);
    }
}
