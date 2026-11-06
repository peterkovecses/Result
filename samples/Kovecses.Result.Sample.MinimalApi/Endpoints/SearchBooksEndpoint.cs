namespace Kovecses.Result.Sample.MinimalApi.Endpoints;

public static class SearchBooksEndpoint
{
    public static RouteHandlerBuilder MapEndpoint(RouteGroupBuilder group)
        => group.MapGet("/books", async (
                string? term,
                int? page,
                int? pageSize,
                IRequestHandler<SearchBooksQuery, Result<PagedResult<BookDto>>> handler,
                CancellationToken cancellationToken) =>
                await handler.HandleAsync(new SearchBooksQuery(term, page ?? 1, pageSize ?? 10), cancellationToken)
                    .ToMinimalApiResultAsync())
            .WithName("SearchBooks")
            .WithDescription("Searches the active catalog with paging.")
            .Produces<PagedResult<BookDto>>(StatusCodes.Status200OK)
            .ProducesValidationProblem();
}
