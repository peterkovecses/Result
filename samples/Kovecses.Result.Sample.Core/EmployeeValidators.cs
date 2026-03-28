namespace Kovecses.Result.Sample.Core;

/// <summary>
/// Separates validation logic from business handlers for a cleaner architecture.
/// </summary>
public sealed class CreateEmployeeValidator : IValidator<CreateEmployeeCommand>
{
    public Task<Result> ValidateAsync(CreateEmployeeCommand request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.FullName))
        {
            return Task.FromResult<Result>(Error.Validation(
                new Dictionary<string, object> { { "FullName", "Name is required." } }));
        }

        if (request.FullName.Length < 3)
        {
            return Task.FromResult<Result>(Error.Validation(
                errors: new Dictionary<string, object> { { "FullName", "Name is too short." } },
                message: "The provided name does not meet the minimum length requirements.",
                code: EmployeeErrorCodes.NameTooShort));
        }

        return Task.FromResult(Result.Success());
    }
}
