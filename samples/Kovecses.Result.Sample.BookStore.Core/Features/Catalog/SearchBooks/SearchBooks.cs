namespace Kovecses.Result.Sample.BookStore.Core.Features.Catalog.SearchBooks;

public sealed record SearchBooksQuery(string? Term, int Page = 1, int PageSize = 10)
    : IRequest<Result<PagedResult<BookDto>>>;

public sealed class SearchBooksValidator : IValidator<SearchBooksQuery>
{
    public const int MaxPageSize = 50;

    public Task<Result> ValidateAsync(SearchBooksQuery request, CancellationToken cancellationToken)
        => new ValidationErrors()
            .AddIf(request.Page < 1, nameof(request.Page), "The page number must be 1 or greater.")
            .AddIf(request.PageSize is < 1 or > MaxPageSize, nameof(request.PageSize), $"The page size must be between 1 and {MaxPageSize}.")
            .ToResultAsync();
}

public sealed class SearchBooksHandler(IBookRepository books)
    : IRequestHandler<SearchBooksQuery, Result<PagedResult<BookDto>>>
{
    public async Task<Result<PagedResult<BookDto>>> HandleAsync(SearchBooksQuery request, CancellationToken cancellationToken)
    {
        var (items, totalCount) = await books.SearchAsync(request.Term, request.Page, request.PageSize, cancellationToken);

        var page = new PagedResult<BookDto>(
            items.Select(BookDto.From).ToList(),
            request.Page,
            request.PageSize,
            totalCount);

        return Result.Success(page);
    }
}
