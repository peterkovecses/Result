namespace Kovecses.Result.Sample.MinimalApi.Endpoints;

public static class AddBookEndpoint
{
    public static RouteHandlerBuilder MapEndpoint(RouteGroupBuilder group)
        => group.MapPost("/books", async (
                AddBookCommand command,
                IRequestHandler<AddBookCommand, Result<BookDto>> handler,
                CancellationToken cancellationToken) =>
            {
                var result = await handler.HandleAsync(command, cancellationToken);
                return result.MatchToMinimalApiResult(book => Results.Created($"/api/catalog/books/{book.Isbn}", book));
            })
            .WithName("AddBook")
            .WithDescription("Adds a book to the catalog.")
            .Produces<BookDto>(StatusCodes.Status201Created)
            .ProducesValidationProblem()
            .ProducesProblem(StatusCodes.Status409Conflict);
}
