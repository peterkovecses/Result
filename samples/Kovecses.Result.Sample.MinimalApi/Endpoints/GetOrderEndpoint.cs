namespace Kovecses.Result.Sample.MinimalApi.Endpoints;

public static class GetOrderEndpoint
{
    public static RouteHandlerBuilder MapEndpoint(RouteGroupBuilder group)
        => group.MapGet("/{orderId:guid}", async (
                Guid orderId,
                IRequestHandler<GetOrderQuery, Result<OrderDto>> handler,
                CancellationToken cancellationToken) =>
                await handler.HandleAsync(new GetOrderQuery(orderId), cancellationToken)
                    .ToMinimalApiResultAsync())
            .WithName("GetOrder")
            .WithDescription("Gets an order owned by the current customer.")
            .Produces<OrderDto>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status403Forbidden)
            .ProducesProblem(StatusCodes.Status404NotFound);
}
