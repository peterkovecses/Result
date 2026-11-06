namespace Kovecses.Result.Sample.MinimalApi.Endpoints;

public static class GetOrderSummaryEndpoint
{
    public static RouteHandlerBuilder MapEndpoint(RouteGroupBuilder group)
        => group.MapGet("/{orderId:guid}/summary", async (
                Guid orderId,
                IRequestHandler<GetOrderSummaryQuery, Result<OrderSummary>> handler,
                CancellationToken cancellationToken) =>
                await handler.HandleAsync(new GetOrderSummaryQuery(orderId), cancellationToken)
                    .ToMinimalApiResultAsync())
            .WithName("GetOrderSummary")
            .WithDescription("Returns a concise, ownership-checked order summary.")
            .Produces<OrderSummary>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status403Forbidden)
            .ProducesProblem(StatusCodes.Status404NotFound);
}
