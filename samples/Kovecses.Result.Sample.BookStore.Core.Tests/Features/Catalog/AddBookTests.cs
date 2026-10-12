using Kovecses.Result.Sample.BookStore.Core.Features.Catalog.AddBook;

namespace Kovecses.Result.Sample.BookStore.Core.Tests.Features.Catalog;

public class AddBookHandlerTests
{
    private readonly InMemoryBookRepository _books = new([TestData.CleanCode()]);
    private readonly AddBookHandler _sut;

    public AddBookHandlerTests() => _sut = new AddBookHandler(_books);

    [Fact]
    public async Task HandleAsync_NewBook_ShouldSaveAndReturnBook()
    {
        // Arrange
        var command = new AddBookCommand(TestData.DddIsbn, "Domain-Driven Design", "Eric Evans", 55m, 5);

        // Act
        var result = await _sut.HandleAsync(command, default);

        // Assert
        result.Should().BeSuccess()
            .HaveData(book => book!.Isbn == TestData.DddIsbn && book.Stock == 5);
        (await _books.GetAsync(TestData.DddIsbn, default)).Should().BeSuccess();
    }

    [Fact]
    public async Task HandleAsync_DuplicateIsbn_ShouldReturnConflict()
    {
        // Arrange
        var command = new AddBookCommand(TestData.CleanCodeIsbn, "Another Title", "Someone", 10m, 1);

        // Act
        var result = await _sut.HandleAsync(command, default);

        // Assert
        result.Should().BeFailure()
            .HaveError(BookErrors.Codes.DuplicateIsbn)
            .HaveType(ErrorType.Conflict);
    }

    [Fact]
    public async Task HandleAsync_NegativeStock_ShouldReturnDomainFailureAndNotSave()
    {
        // Arrange
        var command = new AddBookCommand(TestData.DddIsbn, "Domain-Driven Design", "Eric Evans", 55m, -1);

        // Act
        var result = await _sut.HandleAsync(command, default);

        // Assert
        result.Should().BeFailure()
            .HaveError(BookErrors.Codes.InvalidStock)
            .HaveType(ErrorType.Failure);
        Assert.False(await _books.ExistsAsync(TestData.DddIsbn, default));
    }
}

public class AddBookValidatorTests
{
    private readonly AddBookValidator _sut = new();

    [Fact]
    public async Task ValidateAsync_ValidCommand_ShouldSucceed()
    {
        // Arrange
        var command = new AddBookCommand(TestData.DddIsbn, "Domain-Driven Design", "Eric Evans", 55m, 5);

        // Act
        var result = await _sut.ValidateAsync(command, default);

        // Assert
        result.Should().BeSuccess();
    }

    [Fact]
    public async Task ValidateAsync_EmptyCommand_ShouldReturnEveryFieldError()
    {
        // Arrange
        var command = new AddBookCommand("", "", "", 0m, -1);

        // Act
        var result = await _sut.ValidateAsync(command, default);

        // Assert
        result.Should().BeFailure()
            .HaveError(ErrorCodes.Validation)
            .HaveType(ErrorType.Validation)
            .HaveValidationProperty("Isbn").Contain("required").And
            .HaveValidationProperty("Title").Contain("required").And
            .HaveValidationProperty("Author").Contain("required").And
            .HaveValidationProperty("Price").Contain("greater than zero").And
            .HaveValidationProperty("InitialStock").Contain("negative");
    }

    [Fact]
    public async Task ValidateAsync_MalformedIsbn_ShouldReturnIsbnError()
    {
        // Arrange
        var command = new AddBookCommand("12-34", "Title", "Author", 10m, 1);

        // Act
        var result = await _sut.ValidateAsync(command, default);

        // Assert
        result.Should().BeFailure()
            .HaveError(ErrorCodes.Validation)
            .HaveValidationProperty("Isbn").Contain("13 digits");
    }
}
