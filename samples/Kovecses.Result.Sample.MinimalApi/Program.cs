var builder = WebApplication.CreateBuilder(args);

builder.Services.AddCore();
var app = builder.Build();
var employeesGroup = app.MapGroup("/employees").WithTags("Employees");

// 1. Get - Standard REST (returns data or ProblemDetails)
employeesGroup.MapGet("/{id:int}", async (int id, IMediator mediator, CancellationToken ct) =>
{
    return await mediator.SendAsync(new GetEmployeeQuery(id), ct)
        .ToMinimalApiResultAsync();
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
    return await mediator.SendAsync(new GetEmployeeQuery(id), ct)
        .ToMinimalApiResultAsync(includeResultInResponse: true);
});

// 3. Post - Smart mapping (async, custom success, default failure mapping)
employeesGroup.MapPost("/", async (CreateEmployeeCommand command, IMediator mediator, CancellationToken ct) =>
{
    return await mediator.SendAsync(command, ct)
        .MatchToMinimalApiResultAsync(data => Results.Created($"/employees/{data.Id}", data));
});

// 3b. Post - Smart mapping (sync, custom success, custom failure mapping)
employeesGroup.MapPost("/simple", async (CreateEmployeeCommand command, IMediator mediator, CancellationToken ct) =>
{
    var result = await mediator.SendAsync(command, ct);

    return result.MatchToMinimalApiResult(
        data => Results.Created($"/employees/{data.Id}", data),
        _ => Results.StatusCode(StatusCodes.Status400BadRequest));
});

// 3c. Post - Match with Error[] variant (all errors)
employeesGroup.MapPost("/validate", async (CreateEmployeeCommand command, IMediator mediator, CancellationToken ct) =>
{
    var result = await mediator.SendAsync(command, ct);

    return result.Match(
        data => Results.Created($"/employees/{data.Id}", data),
        errors => Results.BadRequest(new 
        { 
            message = "One or more errors occurred",
            errors = errors.Select(e => new { e.Code, e.Message }) 
        }));
});

// 4. Put - Standard REST (Async Chaining example)
employeesGroup.MapPut("/{id:int}", async (int id, UpdateEmployeeCommand command, IMediator mediator, CancellationToken ct) =>
{
    if (id != command.Id)
    {
        return Results.BadRequest("ID mismatch");
    }

    return await mediator.SendAsync(command, ct)
        .MatchAsync(
            data => Results.Ok(data),
            errors => Result.Failure(errors).ToMinimalApiResult());
});

// 5. Delete - Standard REST (Async mapping)
employeesGroup.MapDelete("/{id:int}", async (int id, IMediator mediator, CancellationToken ct) =>
{
    return await mediator.SendAsync(new DeleteEmployeeCommand(id), ct)
        .ToMinimalApiResultAsync();
});

// 6. Post - Bulk Update (Sync mapping example)
employeesGroup.MapPost("/bulk-update", async (BulkUpdatePositionCommand command, IMediator mediator, CancellationToken ct) =>
{
    var result = await mediator.SendAsync(command, ct);

    if (result.IsFailure)
    {
        // Demonstrating JoinErrorMessages to log or return all errors in a single string
        var allErrors = result.JoinErrorMessages(" | ");
        app.Logger.LogWarning("Bulk update failed: {Errors}", allErrors);
    }

    return result.ToMinimalApiResult();
});

// 7. Get - Summary (demonstrates MatchAsync)
employeesGroup.MapGet("/{id:int}/summary", async (int id, IMediator mediator, CancellationToken ct) =>
{
    var result = await mediator.SendAsync(new GetEmployeeSummaryQuery(id), ct);

    return result.ToMinimalApiResult();
});

// 8. Post - Aggregated Validation with Wrapped Result (demonstrates metadata in full Result response)
employeesGroup.MapPost("/validate-wrapped", async (CreateEmployeeCommand command) =>
{
    var validator = new CreateEmployeeAggregatedValidator();
    var result = await validator.ValidateAsync(command, default);

    return result.ToMinimalApiResult(includeResultInResponse: true);
});

// 9. Get - Direct Error mapping demo
app.MapGet("/health", () => 
    Error.Unexpected("System check failed", "Health.ServiceDown").ToMinimalApiResult());

app.Run();
