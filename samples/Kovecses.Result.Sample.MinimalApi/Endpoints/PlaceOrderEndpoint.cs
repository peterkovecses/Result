namespace Kovecses.Result.Sample.MinimalApi.Endpoints;

public static class PlaceOrderEndpoint
{
    public static RouteHandlerBuilder MapEndpoint(RouteGroupBuilder group)
        => group.MapPost("/", async (
                PlaceOrderCommand command,
                IRequestHandler<PlaceOrderCommand, Result<OrderDto>> handler,
                CancellationToken cancellationToken) =>
                await handler.HandleAsync(command, cancellationToken)
                    .MatchToMinimalApiResultAsync(
                        order => Results.Created($"/api/orders/{order.Id}", order),
                        errors => errors.Any(error => error.Code == OrderErrors.Codes.PaymentDeclined)
                            ? Results.Problem(
                                title: "Payment declined",
                                detail: errors[0].Message,
                                statusCode: StatusCodes.Status402PaymentRequired)
                            : Result.Failure(errors).ToMinimalApiResult()))
            .WithName("PlaceOrder")
            .WithDescription("Places an order, reserves stock, and processes payment.")
            .Produces<OrderDto>(StatusCodes.Status201Created)
            .ProducesValidationProblem()
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status402PaymentRequired)
            .ProducesProblem(StatusCodes.Status403Forbidden)
            .ProducesProblem(StatusCodes.Status408RequestTimeout)
            .ProducesProblem(StatusCodes.Status500InternalServerError);
}
