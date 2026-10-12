
namespace Kovecses.Result.Sample.BookStore.Core.Tests.Features.Catalog;

public class ChangeBookPriceHandlerTests
{
    private readonly InMemoryBookRepository _books = new([TestData.CleanCode()]);
    private readonly ChangeBookPriceHandler _sut;

    public ChangeBookPriceHandlerTests() => _sut = new ChangeBookPriceHandler(_books);

    [Fact]
    public async Task HandleAsync_ExistingBook_ShouldUpdatePrice()
    {
        // Arrange
        var command = new ChangeBookPriceCommand(TestData.CleanCodeIsbn, 45m);

        // Act
        var result = await _sut.HandleAsync(command, default);

        // Assert
        result.Should().BeSuccess()
            .HaveData(book => book!.Price == 45m);
        (await _books.GetAsync(TestData.CleanCodeIsbn, default)).Should().BeSuccess()
            .HaveData(book => book!.Price == 45m);
    }

    [Fact]
    public async Task HandleAsync_UnknownBook_ShouldReturnNotFound()
    {
        // Arrange
        var command = new ChangeBookPriceCommand(TestData.UnknownIsbn, 45m);

        // Act
        var result = await _sut.HandleAsync(command, default);

        // Assert
        result.Should().BeFailure()
            .HaveError(BookErrors.Codes.NotFound);
    }

    [Fact]
    public async Task HandleAsync_RetiredBook_ShouldReturnConflict()
    {
        // Arrange
        await _books.SaveAsync(TestData.CleanCode() with { IsRetired = true }, default);
        var command = new ChangeBookPriceCommand(TestData.CleanCodeIsbn, 45m);

        // Act
        var result = await _sut.HandleAsync(command, default);

        // Assert
        result.Should().BeFailure()
            .HaveError(BookErrors.Codes.Retired)
            .HaveType(ErrorType.Conflict);
    }

    [Fact]
    public async Task HandleAsync_NonPositivePrice_ShouldReturnDomainFailureAndKeepPrice()
    {
        // Arrange
        var command = new ChangeBookPriceCommand(TestData.CleanCodeIsbn, 0m);

        // Act
        var result = await _sut.HandleAsync(command, default);

        // Assert
        result.Should().BeFailure()
            .HaveError(BookErrors.Codes.InvalidPrice);
        (await _books.GetAsync(TestData.CleanCodeIsbn, default)).Should().BeSuccess()
            .HaveData(book => book!.Price == 40m);
    }
}

public class ChangeBookPriceValidatorTests
{
    private readonly ChangeBookPriceValidator _sut = new();

    [Fact]
    public async Task ValidateAsync_PositivePrice_ShouldSucceed()
    {
        // Arrange
        var command = new ChangeBookPriceCommand(TestData.CleanCodeIsbn, 10m);

        // Act
        var result = await _sut.ValidateAsync(command, default);

        // Assert
        result.Should().BeSuccess();
    }

    [Fact]
    public async Task ValidateAsync_NegativePrice_ShouldReturnFieldError()
    {
        // Arrange
        var command = new ChangeBookPriceCommand(TestData.CleanCodeIsbn, -1m);

        // Act
        var result = await _sut.ValidateAsync(command, default);

        // Assert
        result.Should().BeFailure()
            .HaveError(ErrorCodes.Validation)
            .HaveValidationProperty("NewPrice").Contain("greater than zero");
    }
}
