using Kovecses.Result.Sample.BookStore.Core.Features.Catalog.GetBook;

namespace Kovecses.Result.Sample.BookStore.Core.Tests.Features.Catalog;

public class GetBookHandlerTests
{
    private readonly InMemoryBookRepository _books = new([TestData.CleanCode()]);
    private readonly GetBookHandler _sut;

    public GetBookHandlerTests() => _sut = new GetBookHandler(_books);

    [Fact]
    public async Task HandleAsync_ExistingBook_ShouldReturnBook()
    {
        // Arrange
        var query = new GetBookQuery(TestData.CleanCodeIsbn);

        // Act
        var result = await _sut.HandleAsync(query, default);

        // Assert
        result.Should().BeSuccess()
            .HaveData(book => book!.Title == "Clean Code" && book.Stock == 10);
    }

    [Fact]
    public async Task HandleAsync_UnknownBook_ShouldReturnNotFound()
    {
        // Arrange
        var query = new GetBookQuery(TestData.UnknownIsbn);

        // Act
        var result = await _sut.HandleAsync(query, default);

        // Assert
        result.Should().BeFailure()
            .HaveError(BookErrors.Codes.NotFound)
            .HaveType(ErrorType.NotFound);
    }
}
