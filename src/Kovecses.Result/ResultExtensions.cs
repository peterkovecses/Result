namespace Kovecses.Result;

/// <summary>
/// Extension methods for <see cref="Task{TResult}"/> of <see cref="Result"/> to enable fluent async chaining.
/// </summary>
public static class ResultExtensions
{
    /// <summary>
    /// Asynchronously chains another operation if the current task's result is successful.
    /// </summary>
    /// <param name="task">The task returning a result.</param>
    /// <param name="next">The next operation to execute.</param>
    /// <returns>A task representing the result of the next operation, or the current failure.</returns>
    public static async Task<Result> BindAsync(this Task<Result> task, Func<Task<Result>> next)
    {
        var result = await task;

        return result.IsSuccess
            ? await next()
            : result;
    }

    /// <summary>
    /// Asynchronously transforms the success value of the result contained in the task.
    /// </summary>
    /// <typeparam name="TData">The type of the data.</typeparam>
    /// <typeparam name="TNewData">The type of the transformed data.</typeparam>
    /// <param name="task">The task returning a result with data.</param>
    /// <param name="map">The mapping function.</param>
    /// <returns>A task representing the new result with the transformed data, or the current failure.</returns>
    public static async Task<Result<TNewData>> MapAsync<TData, TNewData>(this Task<Result<TData>> task, Func<TData, TNewData> map)
    {
        var result = await task;

        return result.IsSuccess
            ? Result.Success(map(result.Data!))
            : Result.Failure<TNewData>(result.Errors!);
    }

    /// <summary>
    /// Asynchronously transforms the success value of the result contained in the task using an async mapping function.
    /// </summary>
    /// <typeparam name="TData">The type of the data.</typeparam>
    /// <typeparam name="TNewData">The type of the transformed data.</typeparam>
    /// <param name="task">The task returning a result with data.</param>
    /// <param name="map">The asynchronous mapping function.</param>
    /// <returns>A task representing the new result with the transformed data, or the current failure.</returns>
    public static async Task<Result<TNewData>> MapAsync<TData, TNewData>(this Task<Result<TData>> task, Func<TData, Task<TNewData>> map)
    {
        var result = await task;

        return result.IsSuccess
            ? Result.Success(await map(result.Data!))
            : Result.Failure<TNewData>(result.Errors!);
    }

    /// <summary>
    /// Asynchronously chains another result-returning operation if the current task's result is successful.
    /// </summary>
    /// <typeparam name="TData">The type of the data.</typeparam>
    /// <typeparam name="TNewData">The type of the new result data.</typeparam>
    /// <param name="task">The task returning a result with data.</param>
    /// <param name="next">The next operation to execute.</param>
    /// <returns>A task representing the result of the next operation, or the current failure.</returns>
    public static async Task<Result<TNewData>> BindAsync<TData, TNewData>(this Task<Result<TData>> task, Func<TData, Task<Result<TNewData>>> next)
    {
        var result = await task;

        return result.IsSuccess
            ? await next(result.Data!)
            : Result.Failure<TNewData>(result.Errors!);
    }

    /// <summary>
    /// Asynchronously chains another result-returning operation (sync) if the current task's result is successful.
    /// </summary>
    /// <typeparam name="TData">The type of the data.</typeparam>
    /// <typeparam name="TNewData">The type of the new result data.</typeparam>
    /// <param name="task">The task returning a result with data.</param>
    /// <param name="next">The next operation to execute.</param>
    /// <returns>A task representing the result of the next operation, or the current failure.</returns>
    public static async Task<Result<TNewData>> BindAsync<TData, TNewData>(this Task<Result<TData>> task, Func<TData, Result<TNewData>> next)
    {
        var result = await task;

        return result.IsSuccess
            ? next(result.Data!)
            : Result.Failure<TNewData>(result.Errors!);
    }

    /// <summary>
    /// Asynchronously executes an action if the result contained in the task is successful.
    /// </summary>
    /// <param name="task">The task returning a result.</param>
    /// <param name="action">The action to execute.</param>
    /// <returns>A task representing the original result.</returns>
    public static async Task<Result> TapAsync(this Task<Result> task, Action action)
    {
        var result = await task;
        if (result.IsSuccess) action();

        return result;
    }

    /// <summary>
    /// Asynchronously executes a function if the result contained in the task is successful.
    /// </summary>
    /// <param name="task">The task returning a result.</param>
    /// <param name="func">The asynchronous function to execute.</param>
    /// <returns>A task representing the original result.</returns>
    public static async Task<Result> TapAsync(this Task<Result> task, Func<Task> func)
    {
        var result = await task;
        if (result.IsSuccess) await func();

        return result;
    }

    /// <summary>
    /// Asynchronously executes an action with the data if the result contained in the task is successful.
    /// </summary>
    /// <typeparam name="TData">The type of the data.</typeparam>
    /// <param name="task">The task returning a result with data.</param>
    /// <param name="action">The action to execute.</param>
    /// <returns>A task representing the original result.</returns>
    public static async Task<Result<TData>> TapAsync<TData>(this Task<Result<TData>> task, Action<TData> action)
    {
        var result = await task;
        if (result.IsSuccess) action(result.Data!);

        return result;
    }

    /// <summary>
    /// Asynchronously executes a function with the data if the result contained in the task is successful.
    /// </summary>
    /// <typeparam name="TData">The type of the data.</typeparam>
    /// <param name="task">The task returning a result with data.</param>
    /// <param name="func">The asynchronous function to execute.</param>
    /// <returns>A task representing the original result.</returns>
    public static async Task<Result<TData>> TapAsync<TData>(this Task<Result<TData>> task, Func<TData, Task> func)
    {
        var result = await task;
        if (result.IsSuccess) await func(result.Data!);

        return result;
    }

    /// <summary>
    /// Asynchronously executes the appropriate function based on the state of the result contained in the task.
    /// </summary>
    /// <typeparam name="TData">The type of the data.</typeparam>
    /// <typeparam name="TResult">The type of the result after matching.</typeparam>
    /// <param name="task">The task returning a result with data.</param>
    /// <param name="onSuccess">The function to execute on success with the data.</param>
    /// <param name="onFailure">The function to execute on failure with the collection of errors.</param>
    /// <returns>A task representing the result of the executed function.</returns>
    public static async Task<TResult> MatchAsync<TData, TResult>(this Task<Result<TData>> task, Func<TData, TResult> onSuccess, Func<IReadOnlyList<Error>, TResult> onFailure)
    {
        var result = await task;

        return result.IsSuccess
            ? onSuccess(result.Data!)
            : onFailure(result.Errors!);
    }

    /// <summary>
    /// Asynchronously executes the appropriate asynchronous function based on the state of the result contained in the task.
    /// </summary>
    /// <typeparam name="TData">The type of the data.</typeparam>
    /// <typeparam name="TResult">The type of the result after matching.</typeparam>
    /// <param name="task">The task returning a result with data.</param>
    /// <param name="onSuccess">The asynchronous function to execute on success with the data.</param>
    /// <param name="onFailure">The asynchronous function to execute on failure with the collection of errors.</param>
    /// <returns>A task representing the result of the executed function.</returns>
    public static async Task<TResult> MatchAsync<TData, TResult>(this Task<Result<TData>> task, Func<TData, Task<TResult>> onSuccess, Func<IReadOnlyList<Error>, Task<TResult>> onFailure)
    {
        var result = await task;

        return result.IsSuccess
            ? await onSuccess(result.Data!)
            : await onFailure(result.Errors!);
    }

    /// <summary>
    /// Asynchronously executes the appropriate function based on the state of the non-generic result contained in the task.
    /// </summary>
    /// <typeparam name="TResult">The type of the result after matching.</typeparam>
    /// <param name="task">The task returning a result.</param>
    /// <param name="onSuccess">The function to execute on success.</param>
    /// <param name="onFailure">The function to execute on failure with the collection of errors.</param>
    /// <returns>A task representing the result of the executed function.</returns>
    public static async Task<TResult> MatchAsync<TResult>(this Task<Result> task, Func<TResult> onSuccess, Func<IReadOnlyList<Error>, TResult> onFailure)
    {
        var result = await task;

        return result.IsSuccess
            ? onSuccess()
            : onFailure(result.Errors!);
    }

    /// <summary>
    /// Asynchronously executes the appropriate asynchronous function based on the state of the non-generic result contained in the task.
    /// </summary>
    /// <typeparam name="TResult">The type of the result after matching.</typeparam>
    /// <param name="task">The task returning a result.</param>
    /// <param name="onSuccess">The asynchronous function to execute on success.</param>
    /// <param name="onFailure">The asynchronous function to execute on failure with the collection of errors.</param>
    /// <returns>A task representing the result of the executed function.</returns>
    public static async Task<TResult> MatchAsync<TResult>(this Task<Result> task, Func<Task<TResult>> onSuccess, Func<IReadOnlyList<Error>, Task<TResult>> onFailure)
    {
        var result = await task;

        return result.IsSuccess
            ? await onSuccess()
            : await onFailure(result.Errors!);
    }

    /// <summary>
    /// Converts a successful Result to a Result{TData} with the specified data.
    /// </summary>
    /// <typeparam name="TData">The type of the data.</typeparam>
    /// <param name="result">The result to convert.</param>
    /// <param name="data">The data to include in the successful result.</param>
    /// <returns>A Result{TData} with the data if successful, or a failure with the original errors.</returns>
    public static Result<TData> ToResult<TData>(this Result result, TData data)
    {
        return result.IsSuccess
            ? Result.Success(data, result.Metadata)
            : Result.Failure<TData>(result.Errors!, result.Metadata);
    }

    /// <summary>
    /// Converts a Result{TData} to a non-generic Result, discarding the data.
    /// </summary>
    /// <typeparam name="TData">The type of the data.</typeparam>
    /// <param name="result">The result to convert.</param>
    /// <returns>A non-generic Result preserving the success/failure state.</returns>
    public static Result ToResult<TData>(this Result<TData> result)
    {
        return result.IsSuccess
            ? Result.Success(result.Metadata)
            : Result.Failure(result.Errors!, result.Metadata);
    }

    /// <summary>
    /// Ensures a condition is met, otherwise returns a failure with the specified error.
    /// </summary>
    /// <typeparam name="TData">The type of the data.</typeparam>
    /// <param name="result">The result to check.</param>
    /// <param name="predicate">The condition to ensure.</param>
    /// <param name="error">The error to return if the condition is not met.</param>
    /// <returns>The original result if successful and condition is met, otherwise a failure.</returns>
    public static Result<TData> Ensure<TData>(this Result<TData> result, Func<TData, bool> predicate, Error error)
    {
        if (result.IsFailure)
        {
            return result;
        }

        return predicate(result.Data!)
            ? result
            : Result.Failure<TData>(error);
    }

    /// <summary>
    /// Asynchronously ensures a condition is met, otherwise returns a failure with the specified error.
    /// </summary>
    /// <typeparam name="TData">The type of the data.</typeparam>
    /// <param name="task">The task returning a result.</param>
    /// <param name="predicate">The condition to ensure.</param>
    /// <param name="error">The error to return if the condition is not met.</param>
    /// <returns>A task representing the original result if successful and condition is met, otherwise a failure.</returns>
    public static async Task<Result<TData>> EnsureAsync<TData>(this Task<Result<TData>> task, Func<TData, bool> predicate, Error error)
    {
        var result = await task;
        
        return result.Ensure(predicate, error);
    }

    /// <summary>
    /// Asynchronously ensures a condition is met using an async predicate, otherwise returns a failure with the specified error.
    /// </summary>
    /// <typeparam name="TData">The type of the data.</typeparam>
    /// <param name="task">The task returning a result.</param>
    /// <param name="predicate">The async condition to ensure.</param>
    /// <param name="error">The error to return if the condition is not met.</param>
    /// <returns>A task representing the original result if successful and condition is met, otherwise a failure.</returns>
    public static async Task<Result<TData>> EnsureAsync<TData>(this Task<Result<TData>> task, Func<TData, Task<bool>> predicate, Error error)
    {
        var result = await task;

        if (result.IsFailure)
        {
            return result;
        }

        return await predicate(result.Data!)
            ? result
            : Result.Failure<TData>(error);
    }
}