namespace Kovecses.Result.UnitTests;

public class ErrorTests
{
    [Fact]
    public void Constructor_WhenCalled_ShouldInitializeCorrectly()
    {
        // Arrange
        const string code = "User.Conflict";
        const string message = "A conflict occurred.";
        const ErrorType type = ErrorType.Conflict;
        var metadata = new Dictionary<string, object> { { "UserId", 123 } };

        // Act
        var error = new Error(code, message, type, metadata);

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
        var error = Error.Validation(code, message, metadata);

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

    /// <summary>
    /// Demonstrates aggregated validation errors with multiple field-level failures.
    /// This is the recommended pattern for handling bulk validation in APIs.
    /// Metadata keys are property names, values are arrays of validation messages.
    /// </summary>
    [Fact]
    public void Validation_WithAggregatedFieldMetadata_ShouldOrganizeErrorsByProperty()
    {
        // Arrange - 3 validation errors: 2 on Email, 1 on Password
        var validationMetadata = new Dictionary<string, object>
        {
            ["Email"] = new[]
            {
                "Email is required.",
                "Email format is invalid."
            },
            ["Password"] = new[]
            {
                "Password must be at least 8 characters."
            }
        };

        // Act
        var error = Error.Validation(
            ErrorCodes.Validation,
            "Validation failed.",
            validationMetadata
        );

        // Assert
        error.Should().HaveCode(ErrorCodes.Validation)
            .HaveType(ErrorType.Validation)
            .HaveMetadata("Email")
            .HaveMetadata("Password");
    }

    [Fact]
    public void Constructor_WhenCodeIsNull_ShouldThrowArgumentNullException()
    {
        // Arrange
        string code = null!;

        // Act
        var exception = Assert.Throws<ArgumentNullException>(() => new Error(code, "Message", ErrorType.Failure));

        // Assert
        Assert.Equal("code", exception.ParamName);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Constructor_WhenCodeIsEmptyOrWhitespace_ShouldThrowArgumentException(string code)
    {
        // Arrange & Act
        var exception = Assert.Throws<ArgumentException>(() => new Error(code, "Message", ErrorType.Failure));

        // Assert
        Assert.Equal("code", exception.ParamName);
    }

    [Fact]
    public void Constructor_WhenMessageIsNull_ShouldThrowArgumentNullException()
    {
        // Arrange
        string message = null!;

        // Act
        var exception = Assert.Throws<ArgumentNullException>(() => new Error("Code", message, ErrorType.Failure));

        // Assert
        Assert.Equal("message", exception.ParamName);
    }

    [Fact]
    public void Constructor_WhenMessageIsEmpty_ShouldBeValid()
    {
        // Arrange & Act
        var error = new Error("Code", string.Empty, ErrorType.Failure);

        // Assert
        error.Should().HaveMessage(string.Empty);
    }

    [Theory]
    [InlineData(null, "Message", typeof(ArgumentNullException))]
    [InlineData(" ", "Message", typeof(ArgumentException))]
    [InlineData("Code", null, typeof(ArgumentNullException))]
    public void Failure_WhenCodeOrMessageInvalid_ShouldThrow(string? code, string? message, Type expectedException)
    {
        // Arrange & Act
        var exception = Record.Exception(() => Result.Failure(code!, message!));

        // Assert
        Assert.IsType(expectedException, exception);
    }

    [Theory]
    [InlineData(null, "Message", typeof(ArgumentNullException))]
    [InlineData("", "Message", typeof(ArgumentException))]
    [InlineData("Code", null, typeof(ArgumentNullException))]
    public void FailureOfT_WhenCodeOrMessageInvalid_ShouldThrow(string? code, string? message, Type expectedException)
    {
        // Arrange & Act
        var exception = Record.Exception(() => Result.Failure<string>(code!, message!));

        // Assert
        Assert.IsType(expectedException, exception);
    }

    [Theory]
    [InlineData("")]
    [InlineData("  ")]
    public void With_WhenCodeIsEmptyOrWhitespace_ShouldThrowArgumentException(string code)
    {
        // Arrange
        var error = Error.NotFound();

        // Act
        var exception = Record.Exception(() => error with { Code = code });

        // Assert
        Assert.IsType<ArgumentException>(exception);
    }

    [Fact]
    public void With_WhenCodeIsNull_ShouldThrowArgumentNullException()
    {
        // Arrange
        var error = Error.NotFound();

        // Act
        var exception = Record.Exception(() => error with { Code = null! });

        // Assert
        Assert.IsType<ArgumentNullException>(exception);
    }

    [Fact]
    public void With_WhenMessageIsNull_ShouldThrowArgumentNullException()
    {
        // Arrange
        var error = Error.NotFound();

        // Act
        var exception = Record.Exception(() => error with { Message = null! });

        // Assert
        Assert.IsType<ArgumentNullException>(exception);
    }

    [Fact]
    public void With_WhenValidValuesAndMetadata_ShouldCopy()
    {
        // Arrange
        var error = Error.NotFound();
        var metadata = new Dictionary<string, object> { ["Id"] = 1 };

        // Act
        var copy = error with { Code = "New.Code", Message = string.Empty, Metadata = metadata };

        // Assert
        copy.Should().HaveCode("New.Code")
            .HaveMessage(string.Empty)
            .HaveMetadata("Id", 1);
    }

    [Theory]
    [InlineData("{\"message\":\"m\",\"type\":0}")]
    [InlineData("{\"code\":null,\"message\":\"m\"}")]
    [InlineData("{\"code\":\"\",\"message\":\"m\"}")]
    [InlineData("{\"code\":\"  \",\"message\":\"m\"}")]
    [InlineData("{\"code\":\"c\"}")]
    [InlineData("{\"code\":\"c\",\"message\":null}")]
    public void Deserialize_WhenCodeOrMessageInvalid_ShouldThrowJsonException(string json)
    {
        // Arrange & Act
        var exception = Assert.Throws<JsonException>(() => JsonSerializer.Deserialize<Error>(json));

        // Assert
        Assert.IsAssignableFrom<ArgumentException>(exception.InnerException);
    }

    [Fact]
    public void Deserialize_WhenMessageEmptyAndTypeMissing_ShouldDefaultToFailure()
    {
        // Arrange
        const string json = "{\"code\":\"c\",\"message\":\"\"}";

        // Act
        var error = JsonSerializer.Deserialize<Error>(json);

        // Assert
        error.Should().HaveCode("c")
            .HaveMessage(string.Empty)
            .HaveType(ErrorType.Failure);
    }

    [Fact]
    public void Deserialize_WhenResultContainsInvalidError_ShouldThrowJsonException()
    {
        // Arrange
        const string json = "{\"isSuccess\":false,\"errors\":[{\"code\":\"\",\"message\":\"m\",\"type\":0}]}";

        // Act
        var resultException = Record.Exception(() => JsonSerializer.Deserialize<Result>(json));
        var resultOfTException = Record.Exception(() => JsonSerializer.Deserialize<Result<string>>(json));

        // Assert
        Assert.IsType<JsonException>(resultException);
        Assert.IsType<JsonException>(resultOfTException);
    }

    [Fact]
    public void RoundTrip_ForResult_ShouldPreserveError()
    {
        // Arrange
        var original = Result.Failure(Error.Conflict("Taken.", "User.Taken"));

        // Act
        var json = JsonSerializer.Serialize(original);
        var deserialized = JsonSerializer.Deserialize<Result>(json);

        // Assert
        deserialized!.Should().BeFailure()
            .HaveError("User.Taken")
            .HaveMessage("Taken.")
            .HaveType(ErrorType.Conflict);
    }

    [Fact]
    public void RoundTrip_ForResultOfT_ShouldPreserveError()
    {
        // Arrange
        var original = Result.Failure<string>(Error.Conflict("Taken.", "User.Taken"));

        // Act
        var json = JsonSerializer.Serialize(original);
        var deserialized = JsonSerializer.Deserialize<Result<string>>(json);

        // Assert
        deserialized!.Should().BeFailure()
            .HaveError("User.Taken")
            .HaveMessage("Taken.")
            .HaveType(ErrorType.Conflict);
    }
}
