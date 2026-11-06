namespace Kovecses.Result.Sample.MinimalApi.Endpoints;

public static class CancelOrderEndpoint
{
    public static RouteHandlerBuilder MapEndpoint(RouteGroupBuilder group)
        => group.MapPost("/{orderId:guid}/cancel", async (
                Guid orderId,
                IRequestHandler<CancelOrderCommand, Result> handler,
                CancellationToken cancellationToken) =>
                await handler.HandleAsync(new CancelOrderCommand(orderId), cancellationToken)
                    .ToMinimalApiResultAsync())
            .WithName("CancelOrder")
            .WithDescription("Cancels an order owned by the current customer.")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status403Forbidden)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status409Conflict);
}
