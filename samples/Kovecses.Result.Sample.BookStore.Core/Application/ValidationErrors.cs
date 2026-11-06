namespace Kovecses.Result.Sample.BookStore.Core.Application;

/// <summary>
/// Collects field-level validation messages into a single validation error, using the
/// recommended metadata layout (field name to message array).
/// </summary>
public sealed class ValidationErrors
{
    private readonly Dictionary<string, List<string>> _messages = [];

    public ValidationErrors AddIf(bool condition, string field, string message)
    {
        if (!condition)
        {
            return this;
        }

        if (!_messages.TryGetValue(field, out var messages))
        {
            messages = [];
            _messages[field] = messages;
        }

        messages.Add(message);

        return this;
    }

    public Task<Result> ToResultAsync() => Task.FromResult(ToResult());

    public Result ToResult()
    {
        if (_messages.Count == 0)
        {
            return Result.Success();
        }

        var metadata = _messages.ToDictionary(pair => pair.Key, pair => (object)pair.Value.ToArray());

        return Result.Failure(Error.Validation(ErrorCodes.Validation, "One or more validation errors occurred.", metadata));
    }
}
