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
        
        if (request.FullName is { Length: > 0, Length: < 3 })
        {
            errors.Add(Error.Validation("FullName", "Name is too short."));
        }

        if (!string.IsNullOrEmpty(request.FullName) && !request.FullName.Contains(' '))
        {
            errors.Add(Error.Validation("FullName", "Name must contain at least a first and last name separated by a space."));
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

/// <summary>
/// Validator with aggregated validation error metadata for demonstration purposes.
/// </summary>
public sealed class CreateEmployeeAggregatedValidator : IValidator<CreateEmployeeCommand>
{
    public Task<Result> ValidateAsync(CreateEmployeeCommand request, CancellationToken cancellationToken)
    {
        var validationMessages = new Dictionary<string, object>();
        var fullNameMessages = new List<string>();
        var positionMessages = new List<string>();

        // FullName validation
        if (string.IsNullOrWhiteSpace(request.FullName))
        {
            fullNameMessages.Add("Name is required.");
        }
        
        if (request.FullName is { Length: > 0, Length: < 3 })
        {
            fullNameMessages.Add("Name is too short.");
        }

        if (!string.IsNullOrEmpty(request.FullName) && !request.FullName.Contains(' '))
        {
            fullNameMessages.Add("Name must contain at least a first and last name separated by a space.");
        }

        // Position validation
        if (string.IsNullOrWhiteSpace(request.Position))
        {
            positionMessages.Add("Position is required.");
        }

        if (fullNameMessages.Count > 0)
        {
            validationMessages["FullName"] = fullNameMessages;
        }

        if (positionMessages.Count > 0)
        {
            validationMessages["Position"] = positionMessages;
        }

        if (validationMessages.Count > 0)
        {
            return Task.FromResult(Result.Failure(Error.Validation(ErrorCodes.Validation, "Validation failed.", validationMessages)));
        }

        return Task.FromResult(Result.Success());
    }
}
