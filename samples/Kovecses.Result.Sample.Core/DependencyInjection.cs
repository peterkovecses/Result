namespace Kovecses.Result.Sample.Core;

public static class DependencyInjection
{
    public static IServiceCollection AddCore(this IServiceCollection services)
    {
        services.AddSingleton<IMediator, Mediator>();
        
        // Register validators
        services.AddTransient<IValidator<CreateEmployeeCommand>, CreateEmployeeValidator>();
        
        // Register handlers
        services.AddTransient<IRequestHandler<GetEmployeeQuery, Result<Employee>>, EmployeeHandlers>();
        services.AddTransient<IRequestHandler<CreateEmployeeCommand, Result<Employee>>, EmployeeHandlers>();
        services.AddTransient<IRequestHandler<UpdateEmployeeCommand, Result>, EmployeeHandlers>();
        services.AddTransient<IRequestHandler<DeleteEmployeeCommand, Result>, EmployeeHandlers>();
        
        return services;
    }
}
