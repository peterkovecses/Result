using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Shouldly;
using Xunit;

namespace Kovecses.Result.AspNetCore.UnitTests;

public class CoverageTests
{
    [Fact]
    public void GetTitle_WhenUnknownErrorType_ShouldReturnDefaultTitle()
    {
        // Arrange
        var error = Error.Custom("Code", "Message", (ErrorType)999);
        var result = Result.Failure(error);

        // Act
        var apiResult = result.ToMinimalApiResult();

        // Assert
        var problemResult = apiResult.ShouldBeOfType<ProblemHttpResult>();
        problemResult.ProblemDetails.Title.ShouldBe("Bad Request");
    }

    [Fact]
    public void MapToProblem_WhenMultipleErrorsAndNoIncludeResult_ShouldIncludeErrorsInExtensions()
    {
        // Arrange
        var errors = new[] 
        { 
            Error.NotFound("Not Found"), 
            Error.Validation("Key", "Error") 
        };
        var result = Result.Failure(errors);

        // Act
        var apiResult = result.ToMinimalApiResult(includeResultInResponse: false);

        // Assert
        var problemResult = apiResult.ShouldBeOfType<ProblemHttpResult>();
        problemResult.ProblemDetails.Extensions.ShouldContainKey("errors");
    }

    [Fact]
    public void MapToProblem_WhenSingleErrorWithMetadata_ShouldIncludeMetadataInExtensions()
    {
        // Arrange
        var metadata = new Dictionary<string, object> { { "TraceId", "123" } };
        var error = Error.NotFound("Not Found") with { Metadata = metadata };
        var result = Result.Failure(error);

        // Act
        var apiResult = result.ToMinimalApiResult();

        // Assert
        var problemResult = apiResult.ShouldBeOfType<ProblemHttpResult>();
        problemResult.ProblemDetails.Extensions.ShouldContainKey("TraceId");
        problemResult.ProblemDetails.Extensions["TraceId"].ShouldBe("123");
    }

    [Fact]
    public void MapToActionResultProblem_WhenSingleErrorWithMetadata_ShouldIncludeMetadataInExtensions()
    {
        // Arrange
        var metadata = new Dictionary<string, object> { { "TraceId", "123" } };
        var error = Error.Conflict("Conflict") with { Metadata = metadata };
        var result = Result.Failure(error);

        // Act
        var actionResult = result.ToActionResult();

        // Assert
        var objectResult = actionResult.ShouldBeOfType<ObjectResult>();
        var problemDetails = objectResult.Value.ShouldBeOfType<ProblemDetails>();
        problemDetails.Extensions.ShouldContainKey("TraceId");
        problemDetails.Extensions["TraceId"].ShouldBe("123");
    }
}
