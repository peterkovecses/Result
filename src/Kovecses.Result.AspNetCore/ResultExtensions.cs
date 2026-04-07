using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("Kovecses.Result.AspNetCore.UnitTests")]

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
    public static IResult ToMinimalApiResult(this Error error) 
        => MapToProblem(Result.Failure(error), false);

    /// <summary>
    /// Converts an <see cref="Error"/> to an <see cref="IActionResult"/> for Controllers.
    /// </summary>
    /// <param name="error">The error to convert.</param>
    /// <returns>An <see cref="IActionResult"/> representing the error.</returns>
    public static IActionResult ToActionResult(this Error error) 
        => MapToActionResultProblem(Result.Failure(error), false);

    /// <summary>
    /// Asynchronously converts a <see cref="Result"/> to an <see cref="IResult"/> for Minimal APIs.
    /// </summary>
    /// <param name="resultTask">The task returning a result to convert.</param>
    /// <param name="includeResultInResponse">If true, the full <see cref="Result"/> object is returned in the response body.</param>
    /// <returns>A task representing the <see cref="IResult"/>.</returns>
    public static async Task<IResult> ToMinimalApiResultAsync(this Task<Result> resultTask, bool includeResultInResponse = false)
    {
        var result = await resultTask;
        
        return result.ToMinimalApiResult(includeResultInResponse);
    }

    /// <summary>
    /// Asynchronously converts a <see cref="Result{TData}"/> to an <see cref="IResult"/> for Minimal APIs.
    /// </summary>
    /// <typeparam name="TData">The type of the data.</typeparam>
    /// <param name="resultTask">The task returning a result to convert.</param>
    /// <param name="includeResultInResponse">If true, the full <see cref="Result{TData}"/> object is returned in the response body.</param>
    /// <returns>A task representing the <see cref="IResult"/>.</returns>
    public static async Task<IResult> ToMinimalApiResultAsync<TData>(this Task<Result<TData>> resultTask, bool includeResultInResponse = false)
    {
        var result = await resultTask;
        
        return result.ToMinimalApiResult(includeResultInResponse);
    }

    /// <summary>
    /// Asynchronously converts a <see cref="Result"/> to an <see cref="IActionResult"/> for Controllers.
    /// </summary>
    /// <param name="resultTask">The task returning a result to convert.</param>
    /// <param name="includeResultInResponse">If true, the full <see cref="Result"/> object is returned in the response body.</param>
    /// <returns>A task representing the <see cref="IActionResult"/>.</returns>
    public static async Task<IActionResult> ToActionResultAsync(this Task<Result> resultTask, bool includeResultInResponse = false)
    {
        var result = await resultTask;
        
        return result.ToActionResult(includeResultInResponse);
    }

    /// <summary>
    /// Asynchronously converts a <see cref="Result{TData}"/> to an <see cref="IActionResult"/> for Controllers.
    /// </summary>
    /// <typeparam name="TData">The type of the data.</typeparam>
    /// <param name="resultTask">The task returning a result to convert.</param>
    /// <param name="includeResultInResponse">If true, the full <see cref="Result{TData}"/> object is returned in the response body.</param>
    /// <returns>A task representing the <see cref="IActionResult"/>.</returns>
    public static async Task<IActionResult> ToActionResultAsync<TData>(this Task<Result<TData>> resultTask, bool includeResultInResponse = false)
    {
        var result = await resultTask;
        
        return result.ToActionResult(includeResultInResponse);
    }

    private static IResult MapToProblem(Result result, bool includeResultInResponse)
    {
        var errors = result.Errors ?? throw new InvalidOperationException("Failure result must have errors.");
        var firstError = errors[0];
        var statusCode = ErrorResponseHelper.GetStatusCode(firstError.Type);

        if (includeResultInResponse)
        {
            return Results.Json(result, statusCode: statusCode);
        }

        if (errors.All(e => e.Type == ErrorType.Validation))
        {
            return Results.ValidationProblem(
                errors: ErrorResponseHelper.GetValidationDictionary(errors),
                title: "Validation Error",
                statusCode: StatusCodes.Status400BadRequest
            );
        }

        return Results.Problem(
            title: ErrorResponseHelper.GetTitle(firstError.Type),
            detail: firstError.Message,
            statusCode: statusCode,
            extensions: ErrorResponseHelper.GetExtensions(errors, firstError)
        );
    }

    private static ObjectResult MapToActionResultProblem(Result result, bool includeResultInResponse)
    {
        var errors = result.Errors ?? throw new InvalidOperationException("Failure result must have errors.");
        var firstError = errors[0];
        var statusCode = ErrorResponseHelper.GetStatusCode(firstError.Type);

        if (includeResultInResponse)
        {
            return new ObjectResult(result) { StatusCode = statusCode };
        }

        if (errors.All(e => e.Type == ErrorType.Validation))
        {
            var validationProblemDetails = new ValidationProblemDetails(ErrorResponseHelper.GetValidationDictionary(errors))
            {
                Title = "Validation Error",
                Status = StatusCodes.Status400BadRequest
            };
            
            return new ObjectResult(validationProblemDetails) { StatusCode = StatusCodes.Status400BadRequest };
        }

        var problemDetails = new ProblemDetails
        {
            Title = ErrorResponseHelper.GetTitle(firstError.Type),
            Detail = firstError.Message,
            Status = statusCode
        };

        var extensions = ErrorResponseHelper.GetExtensions(errors, firstError);

        if (extensions is not null)
        {
            foreach (var (key, value) in extensions)
            {
                problemDetails.Extensions.Add(key, value);
            }
        }

        return new ObjectResult(problemDetails) { StatusCode = statusCode };
    }
}
