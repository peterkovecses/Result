namespace Kovecses.Result.Sample.BookStore.Core.Domain.Catalog;

public interface IBookRepository
{
    /// <summary>Gets a book by ISBN, or a NotFound failure.</summary>
    Task<Result<Book>> GetAsync(string isbn, CancellationToken cancellationToken);

    Task<bool> ExistsAsync(string isbn, CancellationToken cancellationToken);

    /// <summary>Searches active (not retired) books by title or author.</summary>
    Task<(IReadOnlyList<Book> Items, int TotalCount)> SearchAsync(string? term, int page, int pageSize, CancellationToken cancellationToken);

    Task SaveAsync(Book book, CancellationToken cancellationToken);
}

public static class BookRepositoryExtensions
{
    /// <summary>
    /// Loads several books at once. When some are missing, a single failure with one NotFound error per missing book is returned.
    /// </summary>
    public static async Task<Result<IReadOnlyList<Book>>> GetManyAsync(
        this IBookRepository books,
        IEnumerable<string> isbns,
        CancellationToken cancellationToken)
    {
        var lookups = new List<Result<Book>>();

        foreach (var isbn in isbns)
        {
            lookups.Add(await books.GetAsync(isbn, cancellationToken));
        }

        return Result.Combine(lookups.ToArray()).Match(
            () => Result.Success<IReadOnlyList<Book>>(lookups.Select(lookup => lookup.ValueOrThrow()).ToList()),
            errors => Result.Failure<IReadOnlyList<Book>>(errors));
    }
}
