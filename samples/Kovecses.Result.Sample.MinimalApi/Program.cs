var builder = WebApplication.CreateBuilder(args);
builder.Services.AddCore();
var app = builder.Build();
var employeesGroup = app.MapGroup("/employees").WithTags("Employees");

// 1. Get - Standard REST (returns data or ProblemDetails)
employeesGroup.MapGet("/{id:int}", async (int id, IMediator mediator, CancellationToken ct) =>
{
    var result = await mediator.SendAsync(new GetEmployeeQuery(id), ct);
    
    return result.ToMinimalApiResult();
});

// 2. Get - Wrapped (returns full Result object)
employeesGroup.MapGet("/{id:int}/wrapped", async (int id, IMediator mediator, CancellationToken ct) =>
{
    var result = await mediator.SendAsync(new GetEmployeeQuery(id), ct);

    return result.ToMinimalApiResult(includeResultInResponse: true);
});

// 3. Post - Standard REST (Validation/Conflict demo)
employeesGroup.MapPost("/", async (CreateEmployeeCommand command, IMediator mediator, CancellationToken ct) =>
{
    var result = await mediator.SendAsync(command, ct);

    return result.Match(
        data => Results.Created($"/employees/{data.Id}", data),
        _ => result.ToMinimalApiResult());
});

// 4. Put - Standard REST (No content on success)
employeesGroup.MapPut("/{id:int}", async (int id, UpdateEmployeeCommand command, IMediator mediator, CancellationToken ct) =>
{
    if (id != command.Id) return Results.BadRequest("ID mismatch");
    var result = await mediator.SendAsync(command, ct);
    
    return result.ToMinimalApiResult();
});

// 5. Delete - Standard REST
employeesGroup.MapDelete("/{id:int}", async (int id, IMediator mediator, CancellationToken ct) =>
{
    var result = await mediator.SendAsync(new DeleteEmployeeCommand(id), ct);
    
    return result.ToMinimalApiResult();
});

app.Run();
