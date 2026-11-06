namespace Kovecses.Result.Sample.MinimalApi.Endpoints;

public static class GetBookEndpoint
{
    public static RouteHandlerBuilder MapEndpoint(RouteGroupBuilder group)
        => group.MapGet("/books/{isbn}", async (
                string isbn,
                IRequestHandler<GetBookQuery, Result<BookDto>> handler,
                CancellationToken cancellationToken) =>
            {
                var result = await handler.HandleAsync(new GetBookQuery(isbn), cancellationToken);

                if (result.IsFailure && result.FirstError is { } error)
                {
                    return error.ToMinimalApiResult();
                }

                return result.ToMinimalApiResult();
            })
            .WithName("GetBook")
            .WithDescription("Gets a book by ISBN.")
            .Produces<BookDto>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status404NotFound);
}
