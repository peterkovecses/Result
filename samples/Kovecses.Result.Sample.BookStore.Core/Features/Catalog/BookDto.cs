namespace Kovecses.Result.Sample.BookStore.Core.Features.Catalog;

public sealed record BookDto(string Isbn, string Title, string Author, decimal Price, int Stock, bool IsRetired)
{
    public static BookDto From(Book book)
        => new(book.Isbn, book.Title, book.Author, book.Price, book.Stock, book.IsRetired);
}
