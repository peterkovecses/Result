namespace Kovecses.Result.Sample.BookStore.Core.Features.Catalog.GetBook;

public sealed record GetBookQuery(string Isbn) : IRequest<Result<BookDto>>;

public sealed class GetBookHandler(IBookRepository books) : IRequestHandler<GetBookQuery, Result<BookDto>>
{
    public async Task<Result<BookDto>> HandleAsync(GetBookQuery request, CancellationToken cancellationToken)
        => (await books.GetAsync(request.Isbn, cancellationToken)).Map(BookDto.From);
}
