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

    // 3. Post - Standard REST (Validation/Conflict demo)
    [HttpPost]
    public async Task<IActionResult> Create(CreateEmployeeCommand command, CancellationToken ct)
    {
        var result = await mediator.SendAsync(command, ct);

        return result.Match(
            data => CreatedAtAction(nameof(Get), new { id = data.Id }, data),
            _ => result.ToActionResult());
    }

    // 4. Put - Standard REST (No content on success)
    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, UpdateEmployeeCommand command, CancellationToken ct)
    {
        if (id != command.Id) return BadRequest("ID mismatch");
        var result = await mediator.SendAsync(command, ct);

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
