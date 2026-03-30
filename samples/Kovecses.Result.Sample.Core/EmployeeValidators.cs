namespace Kovecses.Result.Sample.Core;

/// <summary>
/// Separates validation logic from business handlers for a cleaner architecture.
/// </summary>
public sealed class CreateEmployeeValidator : IValidator<CreateEmployeeCommand>
{
    public Task<Result> ValidateAsync(CreateEmployeeCommand request, CancellationToken cancellationToken)
    {
        var errors = new Dictionary<string, object>();

        // FullName validation
        if (string.IsNullOrWhiteSpace(request.FullName))
        {
            errors.Add("FullName", "Name is required.");
        }
        else
        {
            var nameErrors = new List<string>();
            if (request.FullName.Length < 3) nameErrors.Add("Name is too short.");
            if (!request.FullName.Contains(' ')) nameErrors.Add("Name must contain at least a first and last name separated by a space.");

            if (nameErrors.Count > 0)
            {
                errors.Add("FullName", nameErrors);
            }
        }

        // Position validation
        if (string.IsNullOrWhiteSpace(request.Position))
        {
            errors.Add("Position", "Position is required.");
        }

        if (errors.Count > 0)
        {
            return Task.FromResult((Result)Error.Validation(errors));
        }

        return Task.FromResult(Result.Success());
    }
}
