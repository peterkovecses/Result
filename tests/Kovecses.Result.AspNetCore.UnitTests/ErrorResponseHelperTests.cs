using Microsoft.AspNetCore.Http;
using Shouldly;
using Xunit;

namespace Kovecses.Result.AspNetCore.UnitTests;

public class ErrorResponseHelperTests
{
    [Theory]
    [InlineData(ErrorType.Validation, StatusCodes.Status400BadRequest)]
    [InlineData(ErrorType.Failure, StatusCodes.Status400BadRequest)]
    [InlineData(ErrorType.NotFound, StatusCodes.Status404NotFound)]
    [InlineData(ErrorType.Conflict, StatusCodes.Status409Conflict)]
    [InlineData(ErrorType.Unauthorized, StatusCodes.Status401Unauthorized)]
    [InlineData(ErrorType.Forbidden, StatusCodes.Status403Forbidden)]
    [InlineData(ErrorType.Timeout, StatusCodes.Status408RequestTimeout)]
    [InlineData(ErrorType.Unexpected, StatusCodes.Status500InternalServerError)]
    [InlineData(ErrorType.Canceled, StatusCodes.Status400BadRequest)]
    [InlineData((ErrorType)999, StatusCodes.Status400BadRequest)]
    public void GetStatusCode_ShouldMapCorrectly(ErrorType type, int expected)
    {
        ErrorResponseHelper.GetStatusCode(type).ShouldBe(expected);
    }

    [Theory]
    [InlineData(ErrorType.Validation, "Validation Error")]
    [InlineData(ErrorType.NotFound, "Not Found")]
    [InlineData(ErrorType.Conflict, "Conflict")]
    [InlineData((ErrorType)999, "Bad Request")]
    public void GetTitle_ShouldMapCorrectly(ErrorType type, string expected)
    {
        ErrorResponseHelper.GetTitle(type).ShouldBe(expected);
    }

    [Fact]
    public void GetValidationDictionary_ShouldGroupMultipleMessages()
    {
        // Arrange
        var errors = new[]
        {
            Error.Validation("Email", "Required"),
            Error.Validation("Email", "Invalid"),
            Error.Validation("Age", "Too young")
        };

        // Act
        var dict = ErrorResponseHelper.GetValidationDictionary(errors);

        // Assert
        dict.Count.ShouldBe(2);
        dict["Email"].ShouldBe(["Required", "Invalid"]);
        dict["Age"].ShouldBe(["Too young"]);
    }

    [Fact]
    public void GetValidationDictionary_ShouldExtractFromMetadata()
    {
        // Arrange
        var validationMetadata = new Dictionary<string, object>
        {
            ["Email"] = new[] { "Email is required.", "Email format is invalid." },
            ["Password"] = new[] { "Password must be at least 8 characters." }
        };

        var validationError = Error.Validation(
            ErrorCodes.Validation,
            "Validation failed.",
            validationMetadata
        );

        var errors = new[] { validationError };

        // Act
        var dict = ErrorResponseHelper.GetValidationDictionary(errors);

        // Assert
        dict.Count.ShouldBe(2);
        dict["Email"].ShouldBe(["Email is required.", "Email format is invalid."]);
        dict["Password"].ShouldBe(["Password must be at least 8 characters."]);
        dict.ShouldNotContainKey(ErrorCodes.Validation);
    }

    [Fact]
    public void GetValidationDictionary_ShouldMergeCodeAndMetadata()
    {
        // Arrange
        var metadata = new Dictionary<string, object>
        {
            ["Email"] = "Metadata message"
        };

        var errors = new[]
        {
            Error.Validation("Email", "Code message"),
            Error.Validation(ErrorCodes.Validation, "Generic message", metadata)
        };

        // Act
        var dict = ErrorResponseHelper.GetValidationDictionary(errors);

        // Assert
        dict["Email"].ShouldBe(["Code message", "Metadata message"]);
        dict.ShouldNotContainKey(ErrorCodes.Validation);
    }

    [Fact]
    public void GetExtensions_WhenMultipleErrors_ShouldReturnErrorsCollection()
    {
        // Arrange
        var errors = new[] { Error.NotFound(), Error.Conflict() };
        var first = errors[0];

        // Act
        var extensions = ErrorResponseHelper.GetExtensions(errors, first);

        // Assert
        extensions.ShouldNotBeNull();
        extensions.ShouldContainKey("errors");
        extensions["errors"].ShouldBe(errors);
    }

    [Fact]
    public void GetExtensions_WhenSingleErrorNoMetadata_ShouldReturnNull()
    {
        // Arrange
        var errors = new[] { Error.NotFound() };
        var first = errors[0];

        // Act
        var extensions = ErrorResponseHelper.GetExtensions(errors, first);

        // Assert
        extensions.ShouldBeNull();
    }

    [Fact]
    public void GetExtensions_WhenSingleErrorWithMetadata_ShouldCopyMetadata()
    {
        // Arrange
        var metadata = new Dictionary<string, object> { { "Key", "Value" } };
        var first = Error.NotFound() with { Metadata = metadata };
        var errors = new[] { first };

        // Act
        var extensions = ErrorResponseHelper.GetExtensions(errors, first);

        // Assert
        extensions.ShouldNotBeNull();
        extensions.Count.ShouldBe(1);
        extensions["Key"].ShouldBe("Value");
    }
}
