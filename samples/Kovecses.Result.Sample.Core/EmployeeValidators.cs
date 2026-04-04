namespace Kovecses.Result.Sample.Core;

/// <summary>
/// Separates validation logic from business handlers for a cleaner architecture.
/// </summary>
public sealed class CreateEmployeeValidator : IValidator<CreateEmployeeCommand>
{
    public Task<Result> ValidateAsync(CreateEmployeeCommand request, CancellationToken cancellationToken)
    {
        var errors = new List<Error>();

        // FullName validation
        if (string.IsNullOrWhiteSpace(request.FullName))
        {
            errors.Add(Error.Validation("FullName", "Name is required."));
        }
        else
        {
            if (request.FullName.Length < 3)
            {
                errors.Add(Error.Validation("FullName", "Name is too short."));
            }

            if (!request.FullName.Contains(' '))
            {
                errors.Add(Error.Validation("FullName", "Name must contain at least a first and last name separated by a space."));
            }
        }

        // Position validation
        if (string.IsNullOrWhiteSpace(request.Position))
        {
            errors.Add(Error.Validation("Position", "Position is required."));
        }

        if (errors.Count > 0)
        {
            return Task.FromResult(Result.Failure(errors));
        }

        return Task.FromResult(Result.Success());
    }
}
