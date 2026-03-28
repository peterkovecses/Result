namespace Kovecses.Result.Sample.Core;

public sealed record GetEmployeeQuery(int Id) : IRequest<Result<Employee>>;

public sealed record CreateEmployeeCommand(string FullName, string Position) : IRequest<Result<Employee>>;

public sealed record UpdateEmployeeCommand(int Id, string FullName, string Position) : IRequest<Result>;

public sealed record DeleteEmployeeCommand(int Id) : IRequest<Result>;

public sealed class EmployeeHandlers : 
    IRequestHandler<GetEmployeeQuery, Result<Employee>>,
    IRequestHandler<CreateEmployeeCommand, Result<Employee>>,
    IRequestHandler<UpdateEmployeeCommand, Result>,
    IRequestHandler<DeleteEmployeeCommand, Result>
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

    public Task<Result> HandleAsync(UpdateEmployeeCommand request, CancellationToken cancellationToken)
    {
        var employee = Employees.FirstOrDefault(e => e.Id == request.Id);
        if (employee is null)
        {
            return Task.FromResult<Result>(Error.NotFound());
        }

        var index = Employees.IndexOf(employee);
        Employees[index] = employee with { FullName = request.FullName, Position = request.Position };

        return Task.FromResult(Result.Success());
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
}
