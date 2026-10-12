namespace Kovecses.Result.Sample.BookStore.Core.Infrastructure;

public static class BookSeedData
{
    public static IReadOnlyList<Book> Create() =>
    [
        Book.Create("9780132350884", "Clean Code", "Robert C. Martin", 39.99m, 12).ValueOrThrow(),
        Book.Create("9780134494166", "Clean Architecture", "Robert C. Martin", 34.50m, 8).ValueOrThrow(),
        Book.Create("9780321125217", "Domain-Driven Design", "Eric Evans", 54.00m, 5).ValueOrThrow(),
        Book.Create("9781492078005", "Designing Data-Intensive Applications", "Martin Kleppmann", 47.25m, 0).ValueOrThrow(),
        Book.Create("9780135957059", "The Pragmatic Programmer", "David Thomas", 42.00m, 20).ValueOrThrow()
    ];
}
