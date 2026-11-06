namespace Kovecses.Result.Sample.BookStore.Core.Domain.Catalog;

/// <summary>
/// A book in the catalog. Immutable: every state change returns a new instance wrapped in a result.
/// </summary>
public sealed record Book(string Isbn, string Title, string Author, decimal Price, int Stock, bool IsRetired = false)
{
    public static Result<Book> Create(string isbn, string title, string author, decimal price, int stock)
        => Result.Success(new Book(isbn, title, author, price, stock))
            .Ensure(book => book.Price > 0, BookErrors.InvalidPrice())
            .Ensure(book => book.Stock >= 0, BookErrors.InvalidStock());

    public Result<Book> ChangePrice(decimal newPrice)
        => EnsureActive()
            .Ensure(_ => newPrice > 0, BookErrors.InvalidPrice())
            .Map(book => book with { Price = newPrice });

    public Result<Book> Restock(int quantity)
        => EnsureActive()
            .Ensure(_ => quantity > 0, Error.Failure(message: "The restock quantity must be positive.", code: BookErrors.Codes.InvalidStock))
            .Map(book => book with { Stock = book.Stock + quantity });

    public Result<Book> Reserve(int quantity)
        => EnsureActive()
            .Ensure(book => book.Stock >= quantity, BookErrors.InsufficientStock(Isbn, quantity, Stock))
            .Map(book => book with { Stock = book.Stock - quantity });

    public Book Release(int quantity) => this with { Stock = Stock + quantity };

    public Result<Book> Retire()
        => EnsureActive().Map(book => book with { IsRetired = true });

    private Result<Book> EnsureActive()
        => Result.Success(this).Ensure(book => !book.IsRetired, BookErrors.Retired(Isbn));
}
