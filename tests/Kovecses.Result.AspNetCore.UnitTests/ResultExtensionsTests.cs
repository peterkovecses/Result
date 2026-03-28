namespace Kovecses.Result.AspNetCore.UnitTests;

public class ResultExtensionsTests
{
    #region ToMinimalApiResult

    [Fact]
    public void ToMinimalApiResult_WhenSuccess_ShouldReturnNoContent()
    {
        // Arrange
        var result = Result.Success();

        // Act
        var apiResult = result.ToMinimalApiResult();

        // Assert
        apiResult.ShouldBeOfType<NoContent>();
    }

    [Fact]
    public void ToMinimalApiResult_WhenSuccessWithData_ShouldReturnOkWithData()
    {
        // Arrange
        const string data = "TestData";
        var result = Result.Success(data);

        // Act
        var apiResult = result.ToMinimalApiResult();

        // Assert
        var okResult = apiResult.ShouldBeOfType<Ok<string>>();
        okResult.Value.ShouldBe(data);
    }

    [Fact]
    public void ToMinimalApiResult_WhenSuccessWithNullData_ShouldReturnNoContent()
    {
        // Arrange
        var result = Result.Success<string?>(null);

        // Act
        var apiResult = result.ToMinimalApiResult();

        // Assert
        apiResult.ShouldBeOfType<NoContent>();
    }

    [Fact]
    public void ToMinimalApiResult_WhenSuccessAndIncludeResult_ShouldReturnOkWithResult()
    {
        // Arrange
        var result = Result.Success();

        // Act
        var apiResult = result.ToMinimalApiResult(includeResultInResponse: true);

        // Assert
        var okResult = apiResult.ShouldBeOfType<Ok<Result>>();
        okResult.Value.ShouldBe(result);
    }

    [Fact]
    public void ToMinimalApiResult_WhenSuccessWithDataAndIncludeResult_ShouldReturnOkWithResult()
    {
        // Arrange
        const string data = "TestData";
        var result = Result.Success(data);

        // Act
        var apiResult = result.ToMinimalApiResult(includeResultInResponse: true);

        // Assert
        var okResult = apiResult.ShouldBeOfType<Ok<Result<string>>>();
        okResult.Value.ShouldBe(result);
    }

    [Fact]
    public void ToMinimalApiResult_WhenFailure_ShouldReturnProblem()
    {
        // Arrange
        var error = Error.NotFound("Resource not found");
        var result = Result.Failure(error);

        // Act
        var apiResult = result.ToMinimalApiResult();

        // Assert
        var problemResult = apiResult.ShouldBeOfType<ProblemHttpResult>();
        problemResult.StatusCode.ShouldBe(StatusCodes.Status404NotFound);
        problemResult.ProblemDetails.Title.ShouldBe("Not Found");
        problemResult.ProblemDetails.Detail.ShouldBe(error.Message);
    }

    [Fact]
    public void ToMinimalApiResult_WhenFailureAndIncludeResult_ShouldReturnJsonWithResult()
    {
        // Arrange
        var error = Error.NotFound();
        var result = Result.Failure(error);

        // Act
        var apiResult = result.ToMinimalApiResult(includeResultInResponse: true);

        // Assert
        var jsonResult = apiResult.ShouldBeOfType<JsonHttpResult<Result>>();
        jsonResult.StatusCode.ShouldBe(StatusCodes.Status404NotFound);
        jsonResult.Value.ShouldBe(result);
    }
    
    [Theory]
    [InlineData(ErrorType.Failure, StatusCodes.Status400BadRequest)]
    [InlineData(ErrorType.Validation, StatusCodes.Status400BadRequest)]
    [InlineData(ErrorType.NotFound, StatusCodes.Status404NotFound)]
    [InlineData(ErrorType.Conflict, StatusCodes.Status409Conflict)]
    [InlineData(ErrorType.Unauthorized, StatusCodes.Status401Unauthorized)]
    [InlineData(ErrorType.Forbidden, StatusCodes.Status403Forbidden)]
    [InlineData(ErrorType.Timeout, StatusCodes.Status408RequestTimeout)]
    [InlineData(ErrorType.Unexpected, StatusCodes.Status500InternalServerError)]
    public void ToMinimalApiResult_StatusMapping_ShouldReturnCorrectStatusCode(ErrorType errorType, int expectedStatusCode)
    {
        // Arrange
        var error = Error.Custom("Code", "Message", errorType);
        var result = Result.Failure(error);

        // Act
        var apiResult = result.ToMinimalApiResult();

        // Assert
        var problemResult = apiResult.ShouldBeOfType<ProblemHttpResult>();
        problemResult.StatusCode.ShouldBe(expectedStatusCode);
    }

    #endregion

    #region ToActionResult

    [Fact]
    public void ToActionResult_WhenSuccess_ShouldReturnNoContent()
    {
        // Arrange
        var result = Result.Success();

        // Act
        var actionResult = result.ToActionResult();

        // Assert
        actionResult.ShouldBeOfType<NoContentResult>();
    }

    [Fact]
    public void ToActionResult_WhenSuccessWithData_ShouldReturnOkWithData()
    {
        // Arrange
        const int data = 123;
        var result = Result.Success(data);

        // Act
        var actionResult = result.ToActionResult();

        // Assert
        var okResult = actionResult.ShouldBeOfType<OkObjectResult>();
        okResult.Value.ShouldBe(data);
    }

    [Fact]
    public void ToActionResult_WhenSuccessAndIncludeResult_ShouldReturnOkWithResult()
    {
        // Arrange
        var result = Result.Success();

        // Act
        var actionResult = result.ToActionResult(includeResultInResponse: true);

        // Assert
        var okResult = actionResult.ShouldBeOfType<OkObjectResult>();
        okResult.Value.ShouldBe(result);
    }

    [Fact]
    public void ToActionResult_WhenSuccessWithDataAndIncludeResult_ShouldReturnOkWithResult()
    {
        // Arrange
        const string data = "TestData";
        var result = Result.Success(data);

        // Act
        var actionResult = result.ToActionResult(includeResultInResponse: true);

        // Assert
        var okResult = actionResult.ShouldBeOfType<OkObjectResult>();
        okResult.Value.ShouldBe(result);
    }

    [Fact]
    public void ToActionResult_WhenFailure_ShouldReturnProblemDetails()
    {
        // Arrange
        var error = Error.Validation(new Dictionary<string, object> { { "Key", "Error" } });
        var result = Result.Failure(error);

        // Act
        var actionResult = result.ToActionResult();

        // Assert
        var objectResult = actionResult.ShouldBeOfType<ObjectResult>();
        objectResult.StatusCode.ShouldBe(StatusCodes.Status400BadRequest);
        
        var problemDetails = objectResult.Value.ShouldBeOfType<ProblemDetails>();
        problemDetails.Title.ShouldBe("Validation Error");
        problemDetails.Extensions.ContainsKey("Key").ShouldBeTrue();
    }

    [Fact]
    public void ToActionResult_WhenFailureAndIncludeResult_ShouldReturnResultWithCorrectStatusCode()
    {
        // Arrange
        var error = Error.Conflict();
        var result = Result.Failure<string>(error);

        // Act
        var actionResult = result.ToActionResult(includeResultInResponse: true);

        // Assert
        var objectResult = actionResult.ShouldBeOfType<ObjectResult>();
        objectResult.StatusCode.ShouldBe(StatusCodes.Status409Conflict);
        objectResult.Value.ShouldBe(result);
    }

    [Theory]
    [InlineData(ErrorType.Failure, StatusCodes.Status400BadRequest)]
    [InlineData(ErrorType.Validation, StatusCodes.Status400BadRequest)]
    [InlineData(ErrorType.NotFound, StatusCodes.Status404NotFound)]
    [InlineData(ErrorType.Conflict, StatusCodes.Status409Conflict)]
    [InlineData(ErrorType.Unauthorized, StatusCodes.Status401Unauthorized)]
    [InlineData(ErrorType.Forbidden, StatusCodes.Status403Forbidden)]
    [InlineData(ErrorType.Timeout, StatusCodes.Status408RequestTimeout)]
    [InlineData(ErrorType.Unexpected, StatusCodes.Status500InternalServerError)]
    public void ToActionResult_StatusMapping_ShouldReturnCorrectStatusCode(ErrorType errorType, int expectedStatusCode)
    {
        // Arrange
        var error = Error.Custom("Code", "Message", errorType);
        var result = Result.Failure(error);

        // Act
        var actionResult = result.ToActionResult();

        // Assert
        var objectResult = actionResult.ShouldBeOfType<ObjectResult>();
        objectResult.StatusCode.ShouldBe(expectedStatusCode);
    }

    #endregion
}
