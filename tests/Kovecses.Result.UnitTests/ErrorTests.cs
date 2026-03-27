namespace Kovecses.Result.UnitTests;

public class ErrorTests
{
    [Fact]
    public void Custom_WhenCalled_ShouldInitializeCorrectly()
    {
        // Arrange
        var code = "User.Conflict";
        var message = "A conflict occurred.";
        var type = ErrorType.Conflict;
        var metadata = new Dictionary<string, object> { { "UserId", 123 } };

        // Act
        var error = Error.Custom(code, message, type, metadata);

        // Assert
        error.Code.ShouldBe(code);
        error.Message.ShouldBe(message);
        error.Type.ShouldBe(type);
        error.Metadata.ShouldBe(metadata);
    }

    [Fact]
    public void Validation_WhenCalled_ShouldUseCorrectDefaults()
    {
        // Arrange
        var metadata = new Dictionary<string, object> { { "Field", "Required" } };

        // Act
        var error = Error.Validation(metadata);

        // Assert
        error.Code.ShouldBe(ErrorCodes.Validation);
        error.Type.ShouldBe(ErrorType.Validation);
        error.Metadata.ShouldBe(metadata);
    }

    [Fact]
    public void NotFound_WhenCalled_ShouldUseCorrectDefaults()
    {
        // Act
        var error = Error.NotFound();

        // Assert
        error.Code.ShouldBe(ErrorCodes.NotFound);
        error.Type.ShouldBe(ErrorType.NotFound);
    }

    [Fact]
    public void Unauthorized_WhenCalled_ShouldUseCorrectDefaults()
    {
        // Act
        var error = Error.Unauthorized();

        // Assert
        error.Code.ShouldBe(ErrorCodes.Unauthorized);
        error.Type.ShouldBe(ErrorType.Unauthorized);
    }

    [Fact]
    public void Forbidden_WhenCalled_ShouldUseCorrectDefaults()
    {
        // Act
        var error = Error.Forbidden();

        // Assert
        error.Code.ShouldBe(ErrorCodes.Forbidden);
        error.Type.ShouldBe(ErrorType.Forbidden);
    }

    [Fact]
    public void Conflict_WhenCalled_ShouldUseCorrectDefaults()
    {
        // Act
        var error = Error.Conflict();

        // Assert
        error.Code.ShouldBe(ErrorCodes.Conflict);
        error.Type.ShouldBe(ErrorType.Conflict);
    }

    [Fact]
    public void Failure_WhenCalled_ShouldUseCorrectDefaults()
    {
        // Act
        var error = Error.Failure();

        // Assert
        error.Code.ShouldBe(ErrorCodes.Failure);
        error.Type.ShouldBe(ErrorType.Failure);
    }

    [Fact]
    public void Timeout_WhenCalled_ShouldUseCorrectDefaults()
    {
        // Act
        var error = Error.Timeout();

        // Assert
        error.Code.ShouldBe(ErrorCodes.Timeout);
        error.Type.ShouldBe(ErrorType.Timeout);
    }

    [Fact]
    public void Unexpected_WhenCalled_ShouldUseCorrectDefaults()
    {
        // Act
        var error = Error.Unexpected();

        // Assert
        error.Code.ShouldBe(ErrorCodes.Unexpected);
        error.Type.ShouldBe(ErrorType.Unexpected);
    }
}
