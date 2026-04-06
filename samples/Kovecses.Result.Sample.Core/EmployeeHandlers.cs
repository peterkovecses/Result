namespace Kovecses.Result.Sample.Core;

public sealed record GetEmployeeQuery(int Id) : IRequest<Result<Employee>>;

public sealed record CreateEmployeeCommand(string FullName, string Position) : IRequest<Result<Employee>>;

public sealed record UpdateEmployeeCommand(int Id, string FullName, string Position) : IRequest<Result<EmployeeDto>>;

public sealed record BulkUpdatePositionCommand(List<int> Ids, string NewPosition) : IRequest<Result>;

public sealed record DeleteEmployeeCommand(int Id) : IRequest<Result>;

public sealed record GetEmployeeSummaryQuery(int Id) : IRequest<Result<string>>;
public sealed class EmployeeHandlers : 
    IRequestHandler<GetEmployeeQuery, Result<Employee>>,
    IRequestHandler<CreateEmployeeCommand, Result<Employee>>,
    IRequestHandler<UpdateEmployeeCommand, Result<EmployeeDto>>,
    IRequestHandler<DeleteEmployeeCommand, Result>,
    IRequestHandler<BulkUpdatePositionCommand, Result>,
    IRequestHandler<RegisterEmployeeCommand, Result<Employee>>,
    IRequestHandler<GetEmployeeSummaryQuery, Result<string>>
{
    private static readonly List<Employee> Employees =
    [
        new Employee(1, "The Boss", "Chief Executive Officer"),
        new Employee(2, "Jane Smith", "Product Manager")
    ];

    public Task<Result<Employee>> HandleAsync(GetEmployeeQuery request, CancellationToken cancellationToken)
    {
        var employee = Employees.FirstOrDefault(e => e.Id == request.Id);
        
        return employee is null
            ? Task.FromResult<Result<Employee>>(Error.NotFound($"Employee {request.Id} not found."))
            : Task.FromResult<Result<Employee>>(employee);
    }

    public Task<Result<Employee>> HandleAsync(CreateEmployeeCommand request, CancellationToken cancellationToken)
    {
        // Business Rule: Conflict check
        if (Employees.Any(e => e.FullName == request.FullName))
        {
            return Task.FromResult<Result<Employee>>(Error.Conflict($"Employee with name {request.FullName} already exists."));
        }

        var newEmployee = new Employee(Employees.Max(e => e.Id) + 1, request.FullName, request.Position);
        Employees.Add(newEmployee);

        return Task.FromResult<Result<Employee>>(newEmployee);
    }

    public Task<Result<Employee>> HandleAsync(RegisterEmployeeCommand request, CancellationToken cancellationToken)
    {
        // Aggregating multiple domain validation errors into one error 
        // while also keeping track of other structural errors in a single List.
        List<Error> errors = [];

        // 1. Domain Conflict
        if (Employees.Any(e => e.FullName == request.FullName))
        {
            errors.Add(Error.Conflict($"Employee {request.FullName} already exists."));
        }

        // 2. Validation Errors (grouped)
        var validationMetadata = new Dictionary<string, object>();
        if (request.Salary < 0)
            validationMetadata["Salary"] = new[] { "Salary cannot be negative." };

        if (string.IsNullOrWhiteSpace(request.Position))
            validationMetadata["Position"] = new[] { "Position is required." };

        if (validationMetadata.Count > 0)
        {
            errors.Add(Error.Validation(
                code: ErrorCodes.Validation,
                message: "One or more validation errors occurred.",
                metadata: validationMetadata));
        }

        if (errors.Count > 0)
        {
            // Implicitly converts List<Error> to Result<Employee>
            return Task.FromResult<Result<Employee>>(errors);
        }

        var newEmployee = new Employee(Employees.Max(e => e.Id) + 1, request.FullName, request.Position);
        Employees.Add(newEmployee);

        return Task.FromResult<Result<Employee>>(newEmployee);
    }

    public async Task<Result<EmployeeDto>> HandleAsync(UpdateEmployeeCommand request, CancellationToken cancellationToken)
    {
        // Demonstration of Railway-oriented programming:
        // 1. Get existing employee (Async)
        // 2. Validate business rules (Async)
        // 3. Perform update (Async/Sync)
        // 4. Transform to DTO (Sync)
        
        var result = await GetByIdAsync(request.Id)
            .BindAsync(employee => ValidatePositionAsync(employee, request.Position))
            .MapAsync(async employee => 
            {
                var index = Employees.IndexOf(employee);
                var updated = employee with { FullName = request.FullName, Position = request.Position };
                Employees[index] = updated;
                
                return await Task.FromResult(updated);
            })
            .MapAsync(employee => new EmployeeDto(employee.Id, employee.FullName, employee.Position))
            .TapAsync(dto => {
                // Simulating a side effect like logging or notifying other systems
                Console.WriteLine($"[LOG] Employee updated: {dto.DisplayName} ({dto.JobTitle})");
                
                return Task.CompletedTask;
            });

        // Adding audit metadata if successful
        return result.IsSuccess 
            ? Result.Success(result.Data!, new Dictionary<string, object> { { "Audit.Timestamp", DateTime.UtcNow } })
            : Result.Failure<EmployeeDto>(result.Errors!);
    }

    public async Task<Result> HandleAsync(BulkUpdatePositionCommand request, CancellationToken cancellationToken)
    {
        // Demonstration of Result.Combine:
        // Validate each employee update separately and then combine results.
        var tasks = request.Ids.Select(id => UpdateSinglePositionAsync(id, request.NewPosition));
        var results = await Task.WhenAll(tasks);

        return Result.Combine(results);
    }

    private async Task<Result> UpdateSinglePositionAsync(int id, string newPosition)
    {
        var result = await GetByIdAsync(id);
        
        if (result.IsFailure)
        {
            return Result.Failure(result.Errors!);
        }

        var validationResult = await ValidatePositionAsync(result.Data!, newPosition);
        if (validationResult.IsFailure)
        {
            return Result.Failure(validationResult.Errors!);
        }

        var employee = result.Data!;
        var index = Employees.IndexOf(employee);
        Employees[index] = employee with { Position = newPosition };

        return Result.Success();
    }

    private Task<Result<Employee>> GetByIdAsync(int id)
    {
        var employee = Employees.FirstOrDefault(e => e.Id == id);
        
        return Task.FromResult(employee is null 
            ? Result.Failure<Employee>(Error.NotFound($"Employee {id} not found.")) 
            : Result.Success(employee));
    }

    private static Task<Result<Employee>> ValidatePositionAsync(Employee employee, string newPosition)
    {
        // Business Rule: Cannot demote the Boss to a PM
        if (employee.Id == 1 && newPosition == "Product Manager")
        {
            return Task.FromResult<Result<Employee>>(Error.Conflict("The Boss cannot be demoted to a Product Manager."));
        }
        
        return Task.FromResult(Result.Success(employee));
    }

    public Task<Result> HandleAsync(DeleteEmployeeCommand request, CancellationToken cancellationToken)
    {
        var employee = Employees.FirstOrDefault(e => e.Id == request.Id);
        
        if (employee is null)
        {
            return Task.FromResult<Result>(Error.NotFound());
        }

        // Business Rule: Cannot delete the boss
        if (request.Id == 1)
        {
            return Task.FromResult<Result>(Error.Failure(
                message: "The primary administrator (The Boss) cannot be deleted from the system.",
                code: EmployeeErrorCodes.CannotDeleteBoss));
        }

        Employees.Remove(employee);

        return Task.FromResult(Result.Success());
    }

    // Example 1: MatchFirst - handle the first error
    public async Task<Result<string>> HandleAsync(GetEmployeeSummaryQuery request, CancellationToken cancellationToken)
    {
        var result = await GetByIdAsync(request.Id);

        return result.MatchFirst(
            employee => $"Employee {employee.FullName} works as {employee.Position}.",
            error => Result.Failure<string>(error.Code, $"Could not get summary: {result.FirstErrorMessage}", error.Type));
    }

    // Example 2: Match with Error[] - handle all errors
    public async Task<Result<string>> GetEmployeeSummaryWithAllErrorsAsync(int id, CancellationToken cancellationToken)
    {
        var result = await GetByIdAsync(id);

        return result.Match(
            employee => $"Employee {employee.FullName} works as {employee.Position}.",
            errors => Result.Failure<string>(Error.NotFound(result.JoinErrorMessages(", "))));
    }

    // Example 3: Match discard - ignore error details
    public async Task<Result<string>> GetEmployeeSummarySimpleAsync(int id, CancellationToken cancellationToken)
    {
        var result = await GetByIdAsync(id);

        return result.Match(
            employee => $"Employee {employee.FullName} works as {employee.Position}.",
            _ => Result.Failure<string>(Error.NotFound("Could not retrieve employee summary")));
    }
}
