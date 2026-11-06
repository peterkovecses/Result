namespace Kovecses.Result.Sample.Controllers.Controllers;

[ApiController]
[ApiExplorerSettings(IgnoreApi = true)]
[Route("internal/orders")]
public sealed class InternalOrdersController(IMediator mediator) : ApiControllerBase(mediator)
{
    [HttpGet("{orderId:guid}")]
    public Task<IActionResult> Get(Guid orderId, CancellationToken cancellationToken)
        => Mediator.SendAsync<GetOrderQuery, Result<OrderDto>>(new GetOrderQuery(orderId), cancellationToken)
            .ToActionResultAsync(includeResultInResponse: true);
}
