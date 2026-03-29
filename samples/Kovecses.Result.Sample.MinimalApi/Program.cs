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

// 1. Get - ValueOrDefault demo
employeesGroup.MapGet("/{id:int}/name", async (int id, IMediator mediator, CancellationToken ct) =>
{
    var result = await mediator.SendAsync(new GetEmployeeQuery(id), ct);
    
    // Return the name or a default fallback if not found
    return Results.Ok(result.Map(e => e.FullName).ValueOrDefault("Unknown Employee"));
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

// 4. Put - Standard REST (Async Chaining example)
employeesGroup.MapPut("/{id:int}", async (int id, UpdateEmployeeCommand command, IMediator mediator, CancellationToken ct) =>
{
    if (id != command.Id) return Results.BadRequest("ID mismatch");

    return await mediator.SendAsync(command, ct)
        .MatchAsync(
            data => Results.Ok(data),
            error => error.ToMinimalApiResult());
});

// 5. Delete - Standard REST
employeesGroup.MapDelete("/{id:int}", async (int id, IMediator mediator, CancellationToken ct) =>
{
    var result = await mediator.SendAsync(new DeleteEmployeeCommand(id), ct);
    
    return result.ToMinimalApiResult();
});

app.Run();
