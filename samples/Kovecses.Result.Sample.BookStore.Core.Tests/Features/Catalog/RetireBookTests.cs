
namespace Kovecses.Result.Sample.BookStore.Core.Tests.Features.Catalog;

public class RetireBookHandlerTests
{
    private readonly InMemoryBookRepository _books = new([TestData.CleanCode()]);
    private readonly InMemoryOrderRepository _orders = new();
    private readonly RetireBookHandler _sut;

    public RetireBookHandlerTests() => _sut = new RetireBookHandler(_books, _orders);

    [Fact]
    public async Task HandleAsync_BookWithoutOpenOrders_ShouldRetireBook()
    {
        // Arrange
        var command = new RetireBookCommand(TestData.CleanCodeIsbn);

        // Act
        var result = await _sut.HandleAsync(command, default);

        // Assert
        result.Should().BeSuccess();
        (await _books.GetAsync(TestData.CleanCodeIsbn, default)).Should().BeSuccess()
            .HaveData(book => book!.IsRetired);
    }

    [Fact]
    public async Task HandleAsync_UnknownBook_ShouldReturnNotFound()
    {
        // Arrange
        var command = new RetireBookCommand(TestData.UnknownIsbn);

        // Act
        var result = await _sut.HandleAsync(command, default);

        // Assert
        result.Should().BeFailure()
            .HaveError(BookErrors.Codes.NotFound);
    }

    [Fact]
    public async Task HandleAsync_BookWithOpenOrder_ShouldReturnDomainFailure()
    {
        // Arrange
        await _orders.SaveAsync(TestData.OrderFor("alice", TestData.LineOf(TestData.CleanCode(), 1)), default);
        var command = new RetireBookCommand(TestData.CleanCodeIsbn);

        // Act
        var result = await _sut.HandleAsync(command, default);

        // Assert
        result.Should().BeFailure()
            .HaveError(BookErrors.Codes.HasOpenOrders)
            .HaveType(ErrorType.Failure);
    }

    [Fact]
    public async Task HandleAsync_BookWithOnlyCancelledOrder_ShouldRetireBook()
    {
        // Arrange
        var cancelled = TestData.OrderFor("alice", TestData.LineOf(TestData.CleanCode(), 1)).Cancel().ValueOrThrow();
        await _orders.SaveAsync(cancelled, default);
        var command = new RetireBookCommand(TestData.CleanCodeIsbn);

        // Act
        var result = await _sut.HandleAsync(command, default);

        // Assert
        result.Should().BeSuccess();
    }

    [Fact]
    public async Task HandleAsync_AlreadyRetiredBook_ShouldReturnConflict()
    {
        // Arrange
        await _books.SaveAsync(TestData.CleanCode() with { IsRetired = true }, default);
        var command = new RetireBookCommand(TestData.CleanCodeIsbn);

        // Act
        var result = await _sut.HandleAsync(command, default);

        // Assert
        result.Should().BeFailure()
            .HaveError(BookErrors.Codes.Retired)
            .HaveType(ErrorType.Conflict);
    }
}
