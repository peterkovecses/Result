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
    public void NotFound_WhenCalledWithoutArguments_ShouldUseCorrectDefaults()
    {
        // Act
        var error = Error.NotFound();

        // Assert
        error.Should().HaveCode(ErrorCodes.NotFound)
            .HaveMessage("The requested resource was not found.")
            .HaveType(ErrorType.NotFound);
    }

    [Fact]
    public void NotFound_WhenCodeOnlyProvided_ShouldUseCustomCodeAndDefaultMessage()
    {
        // Arrange
        const string code = "Custom.NotFound";

        // Act
        var error = Error.NotFound(code);

        // Assert
        error.Should().HaveCode(code)
            .HaveMessage("The requested resource was not found.")
            .HaveType(ErrorType.NotFound);
    }

    [Fact]
    public void NotFound_WhenCodeAndMessageProvided_ShouldUseCustomValues()
    {
        // Arrange
        const string code = "Custom.NotFound";
        const string message = "Custom not found message.";

        // Act
        var error = Error.NotFound(code, message);

        // Assert
        error.Should().HaveCode(code)
            .HaveMessage(message)
            .HaveType(ErrorType.NotFound);
    }

    [Fact]
    public void NotFound_WhenMessageNamedOnlyProvided_ShouldUseDefaultCodeAndCustomMessage()
    {
        // Arrange
        const string message = "Custom not found message.";

        // Act
        var error = Error.NotFound(message: message);

        // Assert
        error.Should().HaveCode(ErrorCodes.NotFound)
            .HaveMessage(message)
            .HaveType(ErrorType.NotFound);
    }

    [Fact]
    public void Unauthorized_WhenCalledWithoutArguments_ShouldUseCorrectDefaults()
    {
        // Act
        var error = Error.Unauthorized();

        // Assert
        error.Should().HaveCode(ErrorCodes.Unauthorized)
            .HaveMessage("Authentication is required to access this resource.")
            .HaveType(ErrorType.Unauthorized);
    }

    [Fact]
    public void Unauthorized_WhenCodeOnlyProvided_ShouldUseCustomCodeAndDefaultMessage()
    {
        // Arrange
        const string code = "Custom.Unauthorized";

        // Act
        var error = Error.Unauthorized(code);

        // Assert
        error.Should().HaveCode(code)
            .HaveMessage("Authentication is required to access this resource.")
            .HaveType(ErrorType.Unauthorized);
    }

    [Fact]
    public void Unauthorized_WhenCodeAndMessageProvided_ShouldUseCustomValues()
    {
        // Arrange
        const string code = "Custom.Unauthorized";
        const string message = "Custom unauthorized message.";

        // Act
        var error = Error.Unauthorized(code, message);

        // Assert
        error.Should().HaveCode(code)
            .HaveMessage(message)
            .HaveType(ErrorType.Unauthorized);
    }

    [Fact]
    public void Unauthorized_WhenMessageNamedOnlyProvided_ShouldUseDefaultCodeAndCustomMessage()
    {
        // Arrange
        const string message = "Custom unauthorized message.";

        // Act
        var error = Error.Unauthorized(message: message);

        // Assert
        error.Should().HaveCode(ErrorCodes.Unauthorized)
            .HaveMessage(message)
            .HaveType(ErrorType.Unauthorized);
    }

    [Fact]
    public void Forbidden_WhenCalledWithoutArguments_ShouldUseCorrectDefaults()
    {
        // Act
        var error = Error.Forbidden();

        // Assert
        error.Should().HaveCode(ErrorCodes.Forbidden)
            .HaveMessage("You do not have permission to access this resource.")
            .HaveType(ErrorType.Forbidden);
    }

    [Fact]
    public void Forbidden_WhenCodeOnlyProvided_ShouldUseCustomCodeAndDefaultMessage()
    {
        // Arrange
        const string code = "Custom.Forbidden";

        // Act
        var error = Error.Forbidden(code);

        // Assert
        error.Should().HaveCode(code)
            .HaveMessage("You do not have permission to access this resource.")
            .HaveType(ErrorType.Forbidden);
    }

    [Fact]
    public void Forbidden_WhenCodeAndMessageProvided_ShouldUseCustomValues()
    {
        // Arrange
        const string code = "Custom.Forbidden";
        const string message = "Custom forbidden message.";

        // Act
        var error = Error.Forbidden(code, message);

        // Assert
        error.Should().HaveCode(code)
            .HaveMessage(message)
            .HaveType(ErrorType.Forbidden);
    }

    [Fact]
    public void Forbidden_WhenMessageNamedOnlyProvided_ShouldUseDefaultCodeAndCustomMessage()
    {
        // Arrange
        const string message = "Custom forbidden message.";

        // Act
        var error = Error.Forbidden(message: message);

        // Assert
        error.Should().HaveCode(ErrorCodes.Forbidden)
            .HaveMessage(message)
            .HaveType(ErrorType.Forbidden);
    }

    [Fact]
    public void Conflict_WhenCalledWithoutArguments_ShouldUseCorrectDefaults()
    {
        // Act
        var error = Error.Conflict();

        // Assert
        error.Should().HaveCode(ErrorCodes.Conflict)
            .HaveMessage("A conflict occurred while processing the request.")
            .HaveType(ErrorType.Conflict);
    }

    [Fact]
    public void Conflict_WhenCodeOnlyProvided_ShouldUseCustomCodeAndDefaultMessage()
    {
        // Arrange
        const string code = "Custom.Conflict";

        // Act
        var error = Error.Conflict(code);

        // Assert
        error.Should().HaveCode(code)
            .HaveMessage("A conflict occurred while processing the request.")
            .HaveType(ErrorType.Conflict);
    }

    [Fact]
    public void Conflict_WhenCodeAndMessageProvided_ShouldUseCustomValues()
    {
        // Arrange
        const string code = "Custom.Conflict";
        const string message = "Custom conflict message.";

        // Act
        var error = Error.Conflict(code, message);

        // Assert
        error.Should().HaveCode(code)
            .HaveMessage(message)
            .HaveType(ErrorType.Conflict);
    }

    [Fact]
    public void Conflict_WhenMessageNamedOnlyProvided_ShouldUseDefaultCodeAndCustomMessage()
    {
        // Arrange
        const string message = "Custom conflict message.";

        // Act
        var error = Error.Conflict(message: message);

        // Assert
        error.Should().HaveCode(ErrorCodes.Conflict)
            .HaveMessage(message)
            .HaveType(ErrorType.Conflict);
    }

    [Fact]
    public void Failure_WhenCalledWithoutArguments_ShouldUseCorrectDefaults()
    {
        // Act
        var error = Error.Failure();

        // Assert
        error.Should().HaveCode(ErrorCodes.Failure)
            .HaveMessage("The request was invalid or cannot be served.")
            .HaveType(ErrorType.Failure);
    }

    [Fact]
    public void Failure_WhenCodeOnlyProvided_ShouldUseCustomCodeAndDefaultMessage()
    {
        // Arrange
        const string code = "Custom.Failure";

        // Act
        var error = Error.Failure(code);

        // Assert
        error.Should().HaveCode(code)
            .HaveMessage("The request was invalid or cannot be served.")
            .HaveType(ErrorType.Failure);
    }

    [Fact]
    public void Failure_WhenCodeAndMessageProvided_ShouldUseCustomValues()
    {
        // Arrange
        const string code = "Custom.Code";
        const string message = "Custom Message";

        // Act
        var error = Error.Failure(code, message);

        // Assert
        error.Should().HaveCode(code)
            .HaveMessage(message)
            .HaveType(ErrorType.Failure);
    }

    [Fact]
    public void Failure_WhenMessageNamedOnlyProvided_ShouldUseDefaultCodeAndCustomMessage()
    {
        // Arrange
        const string message = "Custom Message";

        // Act
        var error = Error.Failure(message: message);

        // Assert
        error.Should().HaveCode(ErrorCodes.Failure)
            .HaveMessage(message)
            .HaveType(ErrorType.Failure);
    }

    [Fact]
    public void Timeout_WhenCalledWithoutArguments_ShouldUseCorrectDefaults()
    {
        // Act
        var error = Error.Timeout();

        // Assert
        error.Should().HaveCode(ErrorCodes.Timeout)
            .HaveMessage("The operation has timed out.")
            .HaveType(ErrorType.Timeout);
    }

    [Fact]
    public void Timeout_WhenCodeOnlyProvided_ShouldUseCustomCodeAndDefaultMessage()
    {
        // Arrange
        const string code = "Custom.Timeout";

        // Act
        var error = Error.Timeout(code);

        // Assert
        error.Should().HaveCode(code)
            .HaveMessage("The operation has timed out.")
            .HaveType(ErrorType.Timeout);
    }

    [Fact]
    public void Timeout_WhenCodeAndMessageProvided_ShouldUseCustomValues()
    {
        // Arrange
        const string code = "Custom.Timeout";
        const string message = "Custom timeout message.";

        // Act
        var error = Error.Timeout(code, message);

        // Assert
        error.Should().HaveCode(code)
            .HaveMessage(message)
            .HaveType(ErrorType.Timeout);
    }

    [Fact]
    public void Timeout_WhenMessageNamedOnlyProvided_ShouldUseDefaultCodeAndCustomMessage()
    {
        // Arrange
        const string message = "Custom timeout message.";

        // Act
        var error = Error.Timeout(message: message);

        // Assert
        error.Should().HaveCode(ErrorCodes.Timeout)
            .HaveMessage(message)
            .HaveType(ErrorType.Timeout);
    }

    [Fact]
    public void Unexpected_WhenCalledWithoutArguments_ShouldUseCorrectDefaults()
    {
        // Act
        var error = Error.Unexpected();

        // Assert
        error.Should().HaveCode(ErrorCodes.Unexpected)
            .HaveMessage("An unexpected error occurred while processing your request.")
            .HaveType(ErrorType.Unexpected);
    }

    [Fact]
    public void Unexpected_WhenCodeOnlyProvided_ShouldUseCustomCodeAndDefaultMessage()
    {
        // Arrange
        const string code = "Custom.Unexpected";

        // Act
        var error = Error.Unexpected(code);

        // Assert
        error.Should().HaveCode(code)
            .HaveMessage("An unexpected error occurred while processing your request.")
            .HaveType(ErrorType.Unexpected);
    }

    [Fact]
    public void Unexpected_WhenCodeAndMessageProvided_ShouldUseCustomValues()
    {
        // Arrange
        const string code = "Custom.Unexpected";
        const string message = "Custom unexpected message.";

        // Act
        var error = Error.Unexpected(code, message);

        // Assert
        error.Should().HaveCode(code)
            .HaveMessage(message)
            .HaveType(ErrorType.Unexpected);
    }

    [Fact]
    public void Unexpected_WhenMessageNamedOnlyProvided_ShouldUseDefaultCodeAndCustomMessage()
    {
        // Arrange
        const string message = "Custom unexpected message.";

        // Act
        var error = Error.Unexpected(message: message);

        // Assert
        error.Should().HaveCode(ErrorCodes.Unexpected)
            .HaveMessage(message)
            .HaveType(ErrorType.Unexpected);
    }

    [Fact]
    public void Canceled_WhenCalledWithoutArguments_ShouldUseCorrectDefaults()
    {
        // Act
        var error = Error.Canceled();

        // Assert
        error.Should().HaveCode(ErrorCodes.Canceled)
            .HaveMessage("The operation was canceled.")
            .HaveType(ErrorType.Canceled);
    }

    [Fact]
    public void Canceled_WhenCodeOnlyProvided_ShouldUseCustomCodeAndDefaultMessage()
    {
        // Arrange
        const string code = "Custom.Canceled";

        // Act
        var error = Error.Canceled(code);

        // Assert
        error.Should().HaveCode(code)
            .HaveMessage("The operation was canceled.")
            .HaveType(ErrorType.Canceled);
    }

    [Fact]
    public void Canceled_WhenCodeAndMessageProvided_ShouldUseCustomValues()
    {
        // Arrange
        const string code = "Custom.Canceled";
        const string message = "Custom canceled message.";

        // Act
        var error = Error.Canceled(code, message);

        // Assert
        error.Should().HaveCode(code)
            .HaveMessage(message)
            .HaveType(ErrorType.Canceled);
    }

    [Fact]
    public void Canceled_WhenMessageNamedOnlyProvided_ShouldUseDefaultCodeAndCustomMessage()
    {
        // Arrange
        const string message = "Custom canceled message.";

        // Act
        var error = Error.Canceled(message: message);

        // Assert
        error.Should().HaveCode(ErrorCodes.Canceled)
            .HaveMessage(message)
            .HaveType(ErrorType.Canceled);
    }

    public static TheoryData<Func<string, string, Error>, ErrorType> AllFactoriesData => new()
    {
        { (c, m) => Error.NotFound(c, m), ErrorType.NotFound },
        { (c, m) => Error.Unauthorized(c, m), ErrorType.Unauthorized },
        { (c, m) => Error.Forbidden(c, m), ErrorType.Forbidden },
        { (c, m) => Error.Conflict(c, m), ErrorType.Conflict },
        { (c, m) => Error.Failure(c, m), ErrorType.Failure },
        { (c, m) => Error.Timeout(c, m), ErrorType.Timeout },
        { (c, m) => Error.Unexpected(c, m), ErrorType.Unexpected },
        { (c, m) => Error.Canceled(c, m), ErrorType.Canceled },
    };

    [Theory]
    [MemberData(nameof(AllFactoriesData))]
    public void Factory_WhenCalledWithPositionalCodeAndMessage_ShouldYieldCodeFirstAndMessageSecond(
        Func<string, string, Error> factory,
        ErrorType expectedType)
    {
        // Arrange
        const string code = "C";
        const string message = "M";

        // Act
        var error = factory(code, message);

        // Assert
        error.Should().HaveCode(code)
            .HaveMessage(message)
            .HaveType(expectedType);
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
        var original = Result.Failure(Error.Conflict("User.Taken", "Taken."));

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
        var original = Result.Failure<string>(Error.Conflict("User.Taken", "Taken."));

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
