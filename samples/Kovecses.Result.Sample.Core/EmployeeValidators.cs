namespace Kovecses.Result.Sample.Core;

/// <summary>
/// Separates validation logic from business handlers for a cleaner architecture.
/// </summary>
public sealed class CreateEmployeeValidator : IValidator<CreateEmployeeCommand>
{
    public Task<Result> ValidateAsync(CreateEmployeeCommand request, CancellationToken cancellationToken)
    {
        var results = new List<Result>();

        if (string.IsNullOrWhiteSpace(request.FullName))
        {
            results.Add(Error.Validation(new Dictionary<string, object> { { "FullName", "Name is required." } }));
        }
        else if (request.FullName.Length < 3)
        {
            results.Add(Error.Validation(
                errors: new Dictionary<string, object> { { "FullName", "Name is too short." } },
                message: "The provided name does not meet the minimum length requirements.",
                code: EmployeeErrorCodes.NameTooShort));
        }

        if (string.IsNullOrWhiteSpace(request.Position))
        {
            results.Add(Error.Validation(new Dictionary<string, object> { { "Position", "Position is required." } }));
        }

        return Task.FromResult(Result.Combine(results.ToArray()));
    }
}
