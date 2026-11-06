namespace Kovecses.Result.Sample.MinimalApi.Endpoints;

public static class RetireBookEndpoint
{
    public static RouteHandlerBuilder MapEndpoint(RouteGroupBuilder group)
        => group.MapDelete("/books/{isbn}", async (
                string isbn,
                IRequestHandler<RetireBookCommand, Result> handler,
                CancellationToken cancellationToken) =>
                await handler.HandleAsync(new RetireBookCommand(isbn), cancellationToken)
                    .ToMinimalApiResultAsync())
            .WithName("RetireBook")
            .WithDescription("Retires a book when it has no open orders.")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status409Conflict);
}
