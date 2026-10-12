
namespace Kovecses.Result.Sample.BookStore.Core.Infrastructure;

public sealed class InMemoryBookRepository(IEnumerable<Book>? seed = null) : IBookRepository
{
    private readonly ConcurrentDictionary<string, Book> _books = new(
        (seed ?? []).ToDictionary(book => book.Isbn),
        StringComparer.OrdinalIgnoreCase);

    private readonly object _syncRoot = new();

    public Task<Result<Book>> GetAsync(string isbn, CancellationToken cancellationToken)
        => Task.FromResult<Result<Book>>(
            _books.TryGetValue(isbn, out var book) ? book : BookErrors.NotFound(isbn));

    public Task<bool> ExistsAsync(string isbn, CancellationToken cancellationToken)
        => Task.FromResult(_books.ContainsKey(isbn));

    public Task<(IReadOnlyList<Book> Items, int TotalCount)> SearchAsync(
        string? term, int page, int pageSize, CancellationToken cancellationToken)
    {
        var matches = _books.Values
            .Where(book => !book.IsRetired)
            .Where(book => string.IsNullOrWhiteSpace(term)
                || book.Title.Contains(term, StringComparison.OrdinalIgnoreCase)
                || book.Author.Contains(term, StringComparison.OrdinalIgnoreCase))
            .OrderBy(book => book.Title, StringComparer.OrdinalIgnoreCase)
            .ToList();

        IReadOnlyList<Book> items = matches.Skip((page - 1) * pageSize).Take(pageSize).ToList();

        return Task.FromResult((items, matches.Count));
    }

    public Task SaveAsync(Book book, CancellationToken cancellationToken)
    {
        _books[book.Isbn] = book;

        return Task.CompletedTask;
    }

    public Task<Result<IReadOnlyList<Book>>> TryReserveAsync(IReadOnlyList<StockReservationRequest> requests, CancellationToken cancellationToken)
    {
        lock (_syncRoot)
        {
            var reservedBooks = new List<Book>(requests.Count);
            var errors = new List<Error>();

            foreach (var request in requests)
            {
                if (!_books.TryGetValue(request.Isbn, out var book))
                {
                    errors.Add(BookErrors.NotFound(request.Isbn));
                    continue;
                }

                var reservation = book.Reserve(request.Quantity);

                if (reservation.IsFailure)
                {
                    errors.AddRange(reservation.Errors!);
                    continue;
                }

                reservedBooks.Add(reservation.ValueOrThrow());
            }

            if (errors.Count > 0)
            {
                return Task.FromResult<Result<IReadOnlyList<Book>>>(errors);
            }

            foreach (var reservedBook in reservedBooks)
            {
                _books[reservedBook.Isbn] = reservedBook;
            }

            return Task.FromResult<Result<IReadOnlyList<Book>>>(reservedBooks);
        }
    }

    public Task ReleaseAsync(IReadOnlyList<StockReservationRequest> requests, CancellationToken cancellationToken)
    {
        lock (_syncRoot)
        {
            foreach (var request in requests)
            {
                if (!_books.TryGetValue(request.Isbn, out var book))
                {
                    continue;
                }

                // Release is unconditional, so a reservation is returned even if the book was retired meanwhile.
                _books[book.Isbn] = book.Release(request.Quantity);
            }
        }

        return Task.CompletedTask;
    }
}
