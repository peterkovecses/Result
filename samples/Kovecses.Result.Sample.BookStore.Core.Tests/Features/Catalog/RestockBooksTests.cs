using Kovecses.Result.Sample.BookStore.Core.Features.Catalog.RestockBooks;

namespace Kovecses.Result.Sample.BookStore.Core.Tests.Features.Catalog;

public class RestockBooksHandlerTests
{
    private readonly InMemoryBookRepository _books = new([TestData.CleanCode(), TestData.Ddd()]);
    private readonly RestockBooksHandler _sut;

    public RestockBooksHandlerTests()
        => _sut = new RestockBooksHandler(_books, NullLogger<RestockBooksHandler>.Instance);

    [Fact]
    public async Task HandleAsync_ValidItems_ShouldRestockAllBooks()
    {
        // Arrange
        var command = new RestockBooksCommand([new(TestData.CleanCodeIsbn, 5), new(TestData.DddIsbn, 10)]);

        // Act
        var result = await _sut.HandleAsync(command, default);

        // Assert
        result.Should().BeSuccess()
            .HaveMetadata("restockedBooks", 2);
        (await _books.GetAsync(TestData.CleanCodeIsbn, default)).Should().BeSuccess()
            .HaveData(book => book!.Stock == 15);
        (await _books.GetAsync(TestData.DddIsbn, default)).Should().BeSuccess()
            .HaveData(book => book!.Stock == 15);
    }

    [Fact]
    public async Task HandleAsync_UnknownBooks_ShouldReportEveryMissingBook()
    {
        // Arrange
        var command = new RestockBooksCommand([new("1111111111111", 1), new(TestData.CleanCodeIsbn, 1), new("2222222222222", 1)]);

        // Act
        var result = await _sut.HandleAsync(command, default);

        // Assert
        result.Should().BeFailure()
            .HaveErrors().HaveCount(2).AllBeOfType(ErrorType.NotFound);
    }

    [Fact]
    public async Task HandleAsync_RetiredBook_ShouldReturnConflictAndSaveNothing()
    {
        // Arrange
        await _books.SaveAsync(TestData.Ddd() with { IsRetired = true }, default);
        var command = new RestockBooksCommand([new(TestData.CleanCodeIsbn, 5), new(TestData.DddIsbn, 5)]);

        // Act
        var result = await _sut.HandleAsync(command, default);

        // Assert
        result.Should().BeFailure()
            .HaveError(BookErrors.Codes.Retired)
            .HaveType(ErrorType.Conflict);
        (await _books.GetAsync(TestData.CleanCodeIsbn, default)).Should().BeSuccess()
            .HaveData(book => book!.Stock == 10);
    }
}

public class RestockBooksValidatorTests
{
    private readonly RestockBooksValidator _sut = new();

    [Fact]
    public async Task ValidateAsync_ValidItems_ShouldSucceed()
    {
        // Arrange
        var command = new RestockBooksCommand([new(TestData.CleanCodeIsbn, 5)]);

        // Act
        var result = await _sut.ValidateAsync(command, default);

        // Assert
        result.Should().BeSuccess();
    }

    [Fact]
    public async Task ValidateAsync_NoItems_ShouldReturnFieldError()
    {
        // Arrange
        var command = new RestockBooksCommand([]);

        // Act
        var result = await _sut.ValidateAsync(command, default);

        // Assert
        result.Should().BeFailure()
            .HaveError(ErrorCodes.Validation)
            .HaveValidationProperty("Items").Contain("At least one item");
    }

    [Fact]
    public async Task ValidateAsync_BadQuantityAndDuplicates_ShouldReturnAllMessages()
    {
        // Arrange
        var command = new RestockBooksCommand([new(TestData.CleanCodeIsbn, 0), new(TestData.CleanCodeIsbn, 2)]);

        // Act
        var result = await _sut.ValidateAsync(command, default);

        // Assert
        result.Should().BeFailure()
            .HaveError(ErrorCodes.Validation)
            .HaveValidationProperty("Items").ContainAll("quantity", "only appear once");
    }
}
