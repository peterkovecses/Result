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

        return result.IsSuccess ? await next() : result;
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

        return result.IsSuccess ? Result.Success(map(result.Data!)) : Result.Failure<TNewData>(result.Error!);
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

        return result.IsSuccess ? Result.Success(await map(result.Data!)) : Result.Failure<TNewData>(result.Error!);
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

        return result.IsSuccess ? await next(result.Data!) : Result.Failure<TNewData>(result.Error!);
    }

    /// <summary>
    /// Asynchronously executes the appropriate function based on the state of the result contained in the task.
    /// </summary>
    /// <typeparam name="TData">The type of the data.</typeparam>
    /// <typeparam name="TResult">The type of the result after matching.</typeparam>
    /// <param name="task">The task returning a result with data.</param>
    /// <param name="onSuccess">The function to execute on success with the data.</param>
    /// <param name="onFailure">The function to execute on failure with the error.</param>
    /// <returns>A task representing the result of the executed function.</returns>
    public static async Task<TResult> MatchAsync<TData, TResult>(this Task<Result<TData>> task, Func<TData, TResult> onSuccess, Func<Error, TResult> onFailure)
    {
        var result = await task;

        return result.IsSuccess ? onSuccess(result.Data!) : onFailure(result.Error!);
    }

    /// <summary>
    /// Asynchronously executes the appropriate asynchronous function based on the state of the result contained in the task.
    /// </summary>
    /// <typeparam name="TData">The type of the data.</typeparam>
    /// <typeparam name="TResult">The type of the result after matching.</typeparam>
    /// <param name="task">The task returning a result with data.</param>
    /// <param name="onSuccess">The asynchronous function to execute on success with the data.</param>
    /// <param name="onFailure">The asynchronous function to execute on failure with the error.</param>
    /// <returns>A task representing the result of the executed function.</returns>
    public static async Task<TResult> MatchAsync<TData, TResult>(this Task<Result<TData>> task, Func<TData, Task<TResult>> onSuccess, Func<Error, Task<TResult>> onFailure)
    {
        var result = await task;
        
        return result.IsSuccess ? await onSuccess(result.Data!) : await onFailure(result.Error!);
    }
}
