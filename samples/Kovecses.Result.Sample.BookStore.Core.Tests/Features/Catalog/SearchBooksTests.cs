
namespace Kovecses.Result.Sample.BookStore.Core.Tests.Features.Catalog;

public class SearchBooksHandlerTests
{
    private readonly InMemoryBookRepository _books = new(
    [
        TestData.CleanCode(),
        TestData.Ddd(),
        Book.Create("9780135957059", "The Pragmatic Programmer", "David Thomas", 42m, 3).ValueOrThrow()
    ]);

    private readonly SearchBooksHandler _sut;

    public SearchBooksHandlerTests() => _sut = new SearchBooksHandler(_books);

    [Fact]
    public async Task HandleAsync_TermMatchesTitle_ShouldReturnMatchingBooks()
    {
        // Arrange
        var query = new SearchBooksQuery("domain");

        // Act
        var result = await _sut.HandleAsync(query, default);

        // Assert
        result.Should().BeSuccess()
            .HaveData(page => page!.TotalCount == 1 && page.Items.Single().Isbn == TestData.DddIsbn);
    }

    [Fact]
    public async Task HandleAsync_SecondPage_ShouldReturnRemainingBooks()
    {
        // Arrange
        var query = new SearchBooksQuery(null, Page: 2, PageSize: 2);

        // Act
        var result = await _sut.HandleAsync(query, default);

        // Assert
        result.Should().BeSuccess()
            .HaveData(page => page!.Items.Count == 1 && page.TotalCount == 3 && page.TotalPages == 2);
    }

    [Fact]
    public async Task HandleAsync_RetiredBook_ShouldNotBeListed()
    {
        // Arrange
        await _books.SaveAsync(TestData.Ddd() with { IsRetired = true }, default);
        var query = new SearchBooksQuery(null);

        // Act
        var result = await _sut.HandleAsync(query, default);

        // Assert
        result.Should().BeSuccess()
            .HaveData(page => page!.TotalCount == 2 && page.Items.All(book => book.Isbn != TestData.DddIsbn));
    }
}

public class SearchBooksValidatorTests
{
    private readonly SearchBooksValidator _sut = new();

    [Fact]
    public async Task ValidateAsync_DefaultPaging_ShouldSucceed()
    {
        // Arrange
        var query = new SearchBooksQuery("clean");

        // Act
        var result = await _sut.ValidateAsync(query, default);

        // Assert
        result.Should().BeSuccess();
    }

    [Fact]
    public async Task ValidateAsync_InvalidPaging_ShouldReturnFieldErrors()
    {
        // Arrange
        var query = new SearchBooksQuery(null, Page: 0, PageSize: 500);

        // Act
        var result = await _sut.ValidateAsync(query, default);

        // Assert
        result.Should().BeFailure()
            .HaveError(ErrorCodes.Validation)
            .HaveValidationProperty("Page").HaveCount(1);

        result.Should().BeFailure()
            .HaveError(ErrorCodes.Validation)
            .HaveValidationProperty("PageSize").Contain("between 1 and 50");
    }
}
