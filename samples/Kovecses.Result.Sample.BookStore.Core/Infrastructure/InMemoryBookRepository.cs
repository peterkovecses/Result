using System.Collections.Concurrent;

namespace Kovecses.Result.Sample.BookStore.Core.Infrastructure;

public sealed class InMemoryBookRepository(IEnumerable<Book>? seed = null) : IBookRepository
{
    private readonly ConcurrentDictionary<string, Book> _books = new(
        (seed ?? []).ToDictionary(book => book.Isbn),
        StringComparer.OrdinalIgnoreCase);

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
}
