namespace Kovecses.Result.Sample.MinimalApi.Endpoints;

public static class RestockBooksEndpoint
{
    public static RouteHandlerBuilder MapEndpoint(RouteGroupBuilder group)
        => group.MapPost("/books/restock", async (
                RestockBooksCommand command,
                IRequestHandler<RestockBooksCommand, Result> handler,
                CancellationToken cancellationToken) =>
                await handler.HandleAsync(command, cancellationToken).ToMinimalApiResultAsync())
            .WithName("RestockBooks")
            .WithDescription("Restocks one or more books atomically.")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesValidationProblem()
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status409Conflict);
}
