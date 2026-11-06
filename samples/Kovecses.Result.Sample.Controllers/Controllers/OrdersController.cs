namespace Kovecses.Result.Sample.Controllers.Controllers;

[ApiController]
[Route("orders")]
public sealed class OrdersController(IMediator mediator) : ApiControllerBase(mediator)
{
    [HttpPost]
    public async Task<IActionResult> Place(PlaceOrderCommand command, CancellationToken cancellationToken)
    {
        var result = await Mediator.SendAsync<PlaceOrderCommand, Result<OrderDto>>(command, cancellationToken);

        return result.MatchToActionResult(
            order => CreatedAtAction(nameof(Get), new { orderId = order.Id }, order),
            MapPlacementFailure);
    }

    [HttpGet("{orderId:guid}")]
    public Task<IActionResult> Get(Guid orderId, CancellationToken cancellationToken)
        => Mediator.SendAsync<GetOrderQuery, Result<OrderDto>>(new GetOrderQuery(orderId), cancellationToken)
            .ToActionResultAsync();

    [HttpPost("{orderId:guid}/cancel")]
    public Task<IActionResult> Cancel(Guid orderId, CancellationToken cancellationToken)
        => Mediator.SendAsync<CancelOrderCommand, Result>(new CancelOrderCommand(orderId), cancellationToken)
            .ToActionResultAsync();

    [HttpGet("{orderId:guid}/summary")]
    public Task<IActionResult> Summary(Guid orderId, CancellationToken cancellationToken)
        => Mediator.SendAsync<GetOrderSummaryQuery, Result<OrderSummary>>(
                new GetOrderSummaryQuery(orderId),
                cancellationToken)
            .ToActionResultAsync();

    private IActionResult MapPlacementFailure(Error[] errors)
    {
        var paymentDeclined = errors.FirstOrDefault(error => error.Code == OrderErrors.Codes.PaymentDeclined);

        if (paymentDeclined is not null)
        {
            return StatusCode(StatusCodes.Status402PaymentRequired, new ProblemDetails
            {
                Status = StatusCodes.Status402PaymentRequired,
                Title = "Payment declined",
                Detail = paymentDeclined.Message
            });
        }

        return errors.Length == 1
            ? errors[0].ToActionResult()
            : Result.Failure(errors).ToActionResult();
    }
}
