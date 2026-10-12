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
        var ex = Assert.Throws<ArgumentNullException>(() => new Error(null!, "Message", ErrorType.Failure));
        Assert.Equal("code", ex.ParamName);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Constructor_WhenCodeIsEmptyOrWhitespace_ShouldThrowArgumentException(string code)
    {
        var ex = Assert.Throws<ArgumentException>(() => new Error(code, "Message", ErrorType.Failure));
        Assert.Equal("code", ex.ParamName);
    }

    [Fact]
    public void Constructor_WhenMessageIsNull_ShouldThrowArgumentNullException()
    {
        var ex = Assert.Throws<ArgumentNullException>(() => new Error("Code", null!, ErrorType.Failure));
        Assert.Equal("message", ex.ParamName);
    }

    [Fact]
    public void Constructor_WhenMessageIsEmpty_ShouldBeValid()
    {
        var error = new Error("Code", string.Empty, ErrorType.Failure);

        error.Should().HaveMessage(string.Empty);
    }

    [Fact]
    public void Failure_WhenCodeOrMessageInvalid_ShouldThrow()
    {
        Assert.Throws<ArgumentNullException>(() => Result.Failure(null!, "Message"));
        Assert.Throws<ArgumentException>(() => Result.Failure(" ", "Message"));
        Assert.Throws<ArgumentNullException>(() => Result.Failure("Code", null!));
        Assert.Throws<ArgumentNullException>(() => Result.Failure<string>(null!, "Message"));
        Assert.Throws<ArgumentException>(() => Result.Failure<string>("", "Message"));
        Assert.Throws<ArgumentNullException>(() => Result.Failure<string>("Code", null!));
    }

    [Theory]
    [InlineData("")]
    [InlineData("  ")]
    public void With_WhenCodeIsEmptyOrWhitespace_ShouldThrowArgumentException(string code)
    {
        var error = Error.NotFound();

        Assert.Throws<ArgumentException>(() => error with { Code = code });
    }

    [Fact]
    public void With_WhenCodeIsNull_ShouldThrowArgumentNullException()
    {
        var error = Error.NotFound();

        Assert.Throws<ArgumentNullException>(() => error with { Code = null! });
    }

    [Fact]
    public void With_WhenMessageIsNull_ShouldThrowArgumentNullException()
    {
        var error = Error.NotFound();

        Assert.Throws<ArgumentNullException>(() => error with { Message = null! });
    }

    [Fact]
    public void With_WhenValidValuesOrMetadata_ShouldCopy()
    {
        var error = Error.NotFound();
        var metadata = new Dictionary<string, object> { ["Id"] = 1 };

        var copy = error with { Code = "New.Code", Message = "", Metadata = metadata };

        copy.Should().HaveCode("New.Code").HaveMessage("").HaveMetadata("Id", 1);
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
        var ex = Assert.Throws<JsonException>(() => JsonSerializer.Deserialize<Error>(json));

        Assert.IsAssignableFrom<ArgumentException>(ex.InnerException);
    }

    [Fact]
    public void Deserialize_WhenMessageEmptyAndTypeMissing_ShouldDefaultToFailure()
    {
        var error = JsonSerializer.Deserialize<Error>("{\"code\":\"c\",\"message\":\"\"}");

        error.Should().HaveCode("c").HaveMessage("").HaveType(ErrorType.Failure);
    }

    [Fact]
    public void Deserialize_WhenResultContainsInvalidError_ShouldThrowJsonException()
    {
        const string json = "{\"isSuccess\":false,\"errors\":[{\"code\":\"\",\"message\":\"m\",\"type\":0}]}";

        Assert.Throws<JsonException>(() => JsonSerializer.Deserialize<Result>(json));
        Assert.Throws<JsonException>(() => JsonSerializer.Deserialize<Result<string>>(json));
    }

    [Fact]
    public void RoundTrip_ForResultAndResultOfT_ShouldPreserveError()
    {
        var error = Error.Conflict("Taken.", "User.Taken");

        var result = JsonSerializer.Deserialize<Result>(JsonSerializer.Serialize(Result.Failure(error)));
        var resultOfT = JsonSerializer.Deserialize<Result<string>>(JsonSerializer.Serialize(Result.Failure<string>(error)));

        Assert.Equal(error.Code, result!.Errors!.Single().Code);
        Assert.Equal(error.Code, resultOfT!.Errors!.Single().Code);
    }
}
