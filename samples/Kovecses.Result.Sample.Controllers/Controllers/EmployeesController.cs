namespace Kovecses.Result.Sample.Controllers.Controllers;

[ApiController]
[Route("[controller]")]
public class EmployeesController(IMediator mediator) : ControllerBase
{
    // 1. Get - Standard REST (returns data or ProblemDetails)
    [HttpGet("{id:int}")]
    public async Task<IActionResult> Get(int id, CancellationToken ct)
    {
        var result = await mediator.SendAsync(new GetEmployeeQuery(id), ct);

        return result.ToActionResult();
    }

    // 2. Get - Wrapped (returns full Result object)
    [HttpGet("{id:int}/wrapped")]
    public async Task<IActionResult> GetWrapped(int id, CancellationToken ct)
    {
        var result = await mediator.SendAsync(new GetEmployeeQuery(id), ct);

        return result.ToActionResult(includeResultInResponse: true);
    }

    // 2b. Get - Match with all errors (Error[] variant)
    [HttpGet("{id:int}/with-all-errors")]
    public async Task<IActionResult> GetWithAllErrors(int id, CancellationToken ct)
    {
        var result = await mediator.SendAsync(new GetEmployeeQuery(id), ct);

        return result.Match<IActionResult>(
            Ok,
            errors => BadRequest(new 
            { 
                message = "One or more errors occurred",
                errors = errors.Select(e => new { e.Code, e.Message }) 
            }));
    }

    // 3. Post - MatchFirst variant (first error only)
    [HttpPost]
    public async Task<IActionResult> Create(CreateEmployeeCommand command, CancellationToken ct)
    {
        var result = await mediator.SendAsync(command, ct);

        return result.MatchFirst(
            data => CreatedAtAction(nameof(Get), new { id = data.Id }, data),
            error => result.ToActionResult());
    }

    // 3b. Post - Match discard variant (ignore error details)
    [HttpPost("simple")]
    public async Task<IActionResult> CreateSimple(CreateEmployeeCommand command, CancellationToken ct)
    {
        var result = await mediator.SendAsync(command, ct);

        return result.Match(
            data => CreatedAtAction(nameof(Get), new { id = data.Id }, data),
            _ => StatusCode(StatusCodes.Status400BadRequest, "Failed to create employee"));
    }

    // 4. Put - Standard REST (Returns DTO and custom headers from Metadata)
    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, UpdateEmployeeCommand command, CancellationToken ct)
    {
        if (id != command.Id) return BadRequest("ID mismatch");

        var result = await mediator.SendAsync(command, ct);
        
        if (result.IsSuccess && result.Metadata?.TryGetValue("Audit.Timestamp", out var timestamp) == true)
        {
            Response.Headers.Append("X-Audit-Time", timestamp.ToString());
        }

        return result.ToActionResult();
    }

    // 5. Delete - Standard REST
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        var result = await mediator.SendAsync(new DeleteEmployeeCommand(id), ct);

        return result.ToActionResult();
    }
}
