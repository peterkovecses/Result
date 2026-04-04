namespace Kovecses.Result.UnitTests;

public class ErrorTests
{
    [Fact]
    public void Custom_WhenCalled_ShouldInitializeCorrectly()
    {
        // Arrange
        const string code = "User.Conflict";
        const string message = "A conflict occurred.";
        const ErrorType type = ErrorType.Conflict;
        var metadata = new Dictionary<string, object> { { "UserId", 123 } };

        // Act
        var error = Error.Custom(code, message, type, metadata);

        // Assert
        error.Should().HaveCode(code)
            .HaveMessage(message)
            .HaveType(type)
            .HaveMetadata("UserId", 123);
    }

    [Fact]
    public void Validation_WhenCalled_ShouldInitializeCorrectly()
    {
        // Arrange
        const string code = "Validation.Error";
        const string message = "One or more validation errors occurred.";

        // Act
        var error = Error.Validation(code, message);

        // Assert
        error.Should().HaveCode(code)
            .HaveMessage(message)
            .HaveType(ErrorType.Validation);
    }

    [Fact]
    public void Validation_WithMetadata_ShouldInitializeCorrectly()
    {
        // Arrange
        const string code = "Validation.Error";
        const string message = "One or more validation errors occurred.";
        var metadata = new Dictionary<string, object> { { "Field", "Required" } };

        // Act
        var error = Error.Validation(code, message) with { Metadata = metadata };

        // Assert
        error.Should().HaveCode(code)
            .HaveMessage(message)
            .HaveType(ErrorType.Validation)
            .HaveMetadata("Field", "Required");
    }

    [Fact]
    public void NotFound_WhenCalled_ShouldUseCorrectDefaults()
    {
        // Act
        var error = Error.NotFound();

        // Assert
        error.Should().HaveCode(ErrorCodes.NotFound)
            .HaveType(ErrorType.NotFound);
    }

    [Fact]
    public void Unauthorized_WhenCalled_ShouldUseCorrectDefaults()
    {
        // Act
        var error = Error.Unauthorized();

        // Assert
        error.Should().HaveCode(ErrorCodes.Unauthorized)
            .HaveType(ErrorType.Unauthorized);
    }

    [Fact]
    public void Forbidden_WhenCalled_ShouldUseCorrectDefaults()
    {
        // Act
        var error = Error.Forbidden();

        // Assert
        error.Should().HaveCode(ErrorCodes.Forbidden)
            .HaveType(ErrorType.Forbidden);
    }

    [Fact]
    public void Conflict_WhenCalled_ShouldUseCorrectDefaults()
    {
        // Act
        var error = Error.Conflict();

        // Assert
        error.Should().HaveCode(ErrorCodes.Conflict)
            .HaveType(ErrorType.Conflict);
    }

    [Fact]
    public void Failure_WhenCalled_ShouldUseCorrectDefaults()
    {
        // Act
        var error = Error.Failure();

        // Assert
        error.Should().HaveCode(ErrorCodes.Failure)
            .HaveType(ErrorType.Failure);
    }

    [Fact]
    public void Failure_WhenCalledWithParams_ShouldUseCorrectValues()
    {
        // Arrange
        const string code = "Custom.Code";
        const string message = "Custom Message";

        // Act
        var error = Error.Failure(message, code);

        // Assert
        error.Should().HaveCode(code)
            .HaveMessage(message)
            .HaveType(ErrorType.Failure);
    }

    [Fact]
    public void Timeout_WhenCalled_ShouldUseCorrectDefaults()
    {
        // Act
        var error = Error.Timeout();

        // Assert
        error.Should().HaveCode(ErrorCodes.Timeout)
            .HaveType(ErrorType.Timeout);
    }

    [Fact]
    public void Unexpected_WhenCalled_ShouldUseCorrectDefaults()
    {
        // Act
        var error = Error.Unexpected();

        // Assert
        error.Should().HaveCode(ErrorCodes.Unexpected)
            .HaveType(ErrorType.Unexpected);
    }

    [Fact]
    public void Canceled_WhenCalled_ShouldUseCorrectDefaults()
    {
        // Act
        var error = Error.Canceled();

        // Assert
        error.Should().HaveCode(ErrorCodes.Canceled)
            .HaveType(ErrorType.Canceled);
    }
}
