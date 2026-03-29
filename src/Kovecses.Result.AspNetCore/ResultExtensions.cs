using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Kovecses.Result.AspNetCore;

/// <summary>
/// Extension methods for mapping <see cref="Result"/> and <see cref="Result{TData}"/> to ASP.NET Core responses.
/// </summary>
public static class ResultExtensions
{
    /// <summary>
    /// Converts a <see cref="Result"/> to an <see cref="IResult"/> for Minimal APIs.
    /// </summary>
    /// <param name="result">The result to convert.</param>
    /// <param name="includeResultInResponse">If true, the full <see cref="Result"/> object is returned in the response body. Otherwise, only <see cref="ProblemDetails"/> is returned on failure, and no body on success.</param>
    /// <returns>An <see cref="IResult"/> representing the operation outcome.</returns>
    public static IResult ToMinimalApiResult(this Result result, bool includeResultInResponse = false)
    {
        if (!result.IsSuccess)
        {
            return MapToProblem(result, includeResultInResponse);
        }

        return includeResultInResponse 
            ? Results.Ok(result) 
            : Results.NoContent();
    }

    /// <summary>
    /// Converts a <see cref="Result{TData}"/> to an <see cref="IResult"/> for Minimal APIs.
    /// </summary>
    /// <typeparam name="TData">The type of the data.</typeparam>
    /// <param name="result">The result to convert.</param>
    /// <param name="includeResultInResponse">If true, the full <see cref="Result{TData}"/> object is returned in the response body. Otherwise, only the data or <see cref="ProblemDetails"/> is returned.</param>
    /// <returns>An <see cref="IResult"/> representing the operation outcome.</returns>
    public static IResult ToMinimalApiResult<TData>(this Result<TData> result, bool includeResultInResponse = false)
    {
        if (!result.IsSuccess)
        {
            return MapToProblem(result, includeResultInResponse);
        }

        if (includeResultInResponse)
        {
            return Results.Ok(result);
        }

        return result.Data is null 
            ? Results.NoContent()
            : Results.Ok(result.Data);
    }

    /// <summary>
    /// Converts a <see cref="Result"/> to an <see cref="IActionResult"/> for Controllers.
    /// </summary>
    /// <param name="result">The result to convert.</param>
    /// <param name="includeResultInResponse">If true, the full <see cref="Result"/> object is returned in the response body. Otherwise, only <see cref="ProblemDetails"/> is returned on failure, and no body on success.</param>
    /// <returns>An <see cref="IActionResult"/> representing the operation outcome.</returns>
    public static IActionResult ToActionResult(this Result result, bool includeResultInResponse = false)
    {
        if (!result.IsSuccess)
        {
            return MapToActionResultProblem(result, includeResultInResponse);
        }

        return includeResultInResponse 
            ? new OkObjectResult(result) 
            : new NoContentResult();
    }

    /// <summary>
    /// Converts a <see cref="Result{TData}"/> to an <see cref="IActionResult"/> for Controllers.
    /// </summary>
    /// <typeparam name="TData">The type of the data.</typeparam>
    /// <param name="result">The result to convert.</param>
    /// <param name="includeResultInResponse">If true, the full <see cref="Result{TData}"/> object is returned in the response body. Otherwise, only the data or <see cref="ProblemDetails"/> is returned.</param>
    /// <returns>An <see cref="IActionResult"/> representing the operation outcome.</returns>
    public static IActionResult ToActionResult<TData>(this Result<TData> result, bool includeResultInResponse = false)
    {
        if (!result.IsSuccess)
        {
            return MapToActionResultProblem(result, includeResultInResponse);
        }

        if (includeResultInResponse)
        {
            return new OkObjectResult(result);
        }

        return result.Data is null 
            ? new NoContentResult() 
            : new OkObjectResult(result.Data);
    }

    /// <summary>
    /// Converts an <see cref="Error"/> to an <see cref="IResult"/> for Minimal APIs.
    /// </summary>
    /// <param name="error">The error to convert.</param>
    /// <returns>An <see cref="IResult"/> representing the error.</returns>
    public static IResult ToMinimalApiResult(this Error error) => MapToProblem(Result.Failure(error), false);

    /// <summary>
    /// Converts an <see cref="Error"/> to an <see cref="IActionResult"/> for Controllers.
    /// </summary>
    /// <param name="error">The error to convert.</param>
    /// <returns>An <see cref="IActionResult"/> representing the error.</returns>
    public static IActionResult ToActionResult(this Error error) => MapToActionResultProblem(Result.Failure(error), false);

    private static IResult MapToProblem(Result result, bool includeResultInResponse)
    {
        var error = result.Error ?? throw new InvalidOperationException("Failure result must have an error.");
        var statusCode = GetStatusCode(error.Type);

        if (includeResultInResponse)
        {
            return Results.Json(result, statusCode: statusCode);
        }

        return Results.Problem(
            title: GetTitle(error.Type),
            detail: error.Message,
            statusCode: statusCode,
            extensions: error.Metadata?.ToDictionary(x => x.Key, x => (object?)x.Value)
        );
    }

    private static ObjectResult MapToActionResultProblem(Result result, bool includeResultInResponse)
    {
        var error = result.Error ?? throw new InvalidOperationException("Failure result must have an error.");
        var statusCode = GetStatusCode(error.Type);

        if (includeResultInResponse)
        {
            return new ObjectResult(result) { StatusCode = statusCode };
        }

        var problemDetails = new ProblemDetails
        {
            Title = GetTitle(error.Type),
            Detail = error.Message,
            Status = statusCode
        };

        if (error.Metadata != null)
        {
            foreach (var kvp in error.Metadata)
            {
                problemDetails.Extensions.Add(kvp.Key, kvp.Value);
            }
        }

        return new ObjectResult(problemDetails) { StatusCode = statusCode };
    }

    private static int GetStatusCode(ErrorType type) => type switch
    {
        ErrorType.Validation => StatusCodes.Status400BadRequest,
        ErrorType.NotFound => StatusCodes.Status404NotFound,
        ErrorType.Conflict => StatusCodes.Status409Conflict,
        ErrorType.Unauthorized => StatusCodes.Status401Unauthorized,
        ErrorType.Forbidden => StatusCodes.Status403Forbidden,
        ErrorType.Timeout => StatusCodes.Status408RequestTimeout,
        ErrorType.Unexpected => StatusCodes.Status500InternalServerError,
        _ => StatusCodes.Status400BadRequest
    };

    private static string GetTitle(ErrorType type) => type switch
    {
        ErrorType.Validation => "Validation Error",
        ErrorType.NotFound => "Not Found",
        ErrorType.Conflict => "Conflict",
        ErrorType.Unauthorized => "Unauthorized",
        ErrorType.Forbidden => "Forbidden",
        ErrorType.Timeout => "Request Timeout",
        ErrorType.Unexpected => "Internal Server Error",
        _ => "Bad Request"
    };
}
