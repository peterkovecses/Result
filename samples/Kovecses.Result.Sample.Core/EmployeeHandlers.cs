namespace Kovecses.Result.Sample.Core;

public record GetEmployeeQuery(int Id) : IRequest<Result<Employee>>;

public record CreateEmployeeCommand(string FullName, string Position) : IRequest<Result<Employee>>;

public record UpdateEmployeeCommand(int Id, string FullName, string Position) : IRequest<Result>;

public record DeleteEmployeeCommand(int Id) : IRequest<Result>;

public class EmployeeHandlers : 
    IRequestHandler<GetEmployeeQuery, Result<Employee>>,
    IRequestHandler<CreateEmployeeCommand, Result<Employee>>,
    IRequestHandler<UpdateEmployeeCommand, Result>,
    IRequestHandler<DeleteEmployeeCommand, Result>
{
    private static readonly List<Employee> _employees =
    [
        new Employee(1, "John Doe", "Software Engineer"),
        new Employee(2, "Jane Smith", "Product Manager")
    ];

    public Task<Result<Employee>> HandleAsync(GetEmployeeQuery request, CancellationToken cancellationToken)
    {
        // 1. Success with Data (Implicit conversion)
        var employee = _employees.FirstOrDefault(e => e.Id == request.Id);
        
        if (employee is null)
        {
            // 2. Failure: NotFound (Implicit conversion from Error)
            return Task.FromResult<Result<Employee>>(Error.NotFound($"Employee with ID {request.Id} was not found."));
        }

        return Task.FromResult<Result<Employee>>(employee);
    }

    public Task<Result<Employee>> HandleAsync(CreateEmployeeCommand request, CancellationToken cancellationToken)
    {
        // 3. Failure: Validation (Implicit conversion from Error)
        if (string.IsNullOrWhiteSpace(request.FullName))
        {
            var errors = new Dictionary<string, object>
            {
                { "FullName", "Full name is required." }
            };

            return Task.FromResult<Result<Employee>>(Error.Validation(errors));
        }

        // 4. Failure: Conflict (Implicit conversion from Error)
        if (_employees.Any(e => e.FullName == request.FullName))
        {
            return Task.FromResult<Result<Employee>>(Error.Conflict($"Employee with name {request.FullName} already exists."));
        }

        var newEmployee = new Employee(_employees.Max(e => e.Id) + 1, request.FullName, request.Position);
        _employees.Add(newEmployee);

        return Task.FromResult<Result<Employee>>(newEmployee);
    }

    public Task<Result> HandleAsync(UpdateEmployeeCommand request, CancellationToken cancellationToken)
    {
        var employee = _employees.FirstOrDefault(e => e.Id == request.Id);
        if (employee is null)
        {
            return Task.FromResult<Result>(Error.NotFound());
        }

        // 5. Success without Data
        var index = _employees.IndexOf(employee);
        _employees[index] = employee with { FullName = request.FullName, Position = request.Position };

        return Task.FromResult(Result.Success());
    }

    public Task<Result> HandleAsync(DeleteEmployeeCommand request, CancellationToken cancellationToken)
    {
        var employee = _employees.FirstOrDefault(e => e.Id == request.Id);
        if (employee is null)
        {
            return Task.FromResult<Result>(Error.NotFound());
        }

        _employees.Remove(employee);

        return Task.FromResult(Result.Success());
    }
}
