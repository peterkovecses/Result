
namespace Kovecses.Result.Sample.BookStore.Core.Tests.Features.Orders;

public class CancelOrderHandlerTests
{
    private readonly InMemoryBookRepository _books = new([TestData.CleanCode(stock: 8)]);
    private readonly InMemoryOrderRepository _orders = new();
    private readonly TestCurrentUser _user = new();
    private readonly CancelOrderHandler _sut;

    public CancelOrderHandlerTests() => _sut = new CancelOrderHandler(_orders, _books, _user);

    private async Task<Order> SaveOrderAsync(string customerId)
    {
        var order = TestData.OrderFor(customerId, TestData.LineOf(TestData.CleanCode(), 2));
        await _orders.SaveAsync(order, default);

        return order;
    }

    [Fact]
    public async Task HandleAsync_OwnOpenOrder_ShouldCancelAndRestoreStock()
    {
        // Arrange
        var order = await SaveOrderAsync("alice");

        // Act
        var result = await _sut.HandleAsync(new CancelOrderCommand(order.Id), default);

        // Assert
        result.Should().BeSuccess()
            .HaveMetadata("refundAmount", 80m);
        (await _orders.GetAsync(order.Id, default)).Should().BeSuccess()
            .HaveData(saved => saved!.Status == OrderStatus.Cancelled);
        (await _books.GetAsync(TestData.CleanCodeIsbn, default)).Should().BeSuccess()
            .HaveData(book => book!.Stock == 10);
    }

    [Fact]
    public async Task HandleAsync_AnonymousUser_ShouldReturnUnauthorized()
    {
        // Arrange
        _user.UserId = null;

        // Act
        var result = await _sut.HandleAsync(new CancelOrderCommand(Guid.NewGuid()), default);

        // Assert
        result.Should().BeFailure()
            .HaveError(ErrorCodes.Unauthorized);
    }

    [Fact]
    public async Task HandleAsync_UnknownOrder_ShouldReturnNotFound()
    {
        // Arrange
        var command = new CancelOrderCommand(Guid.NewGuid());

        // Act
        var result = await _sut.HandleAsync(command, default);

        // Assert
        result.Should().BeFailure()
            .HaveError(OrderErrors.Codes.NotFound);
    }

    [Fact]
    public async Task HandleAsync_OrderOfAnotherCustomer_ShouldReturnForbiddenAndKeepOrderOpen()
    {
        // Arrange
        var order = await SaveOrderAsync("bob");

        // Act
        var result = await _sut.HandleAsync(new CancelOrderCommand(order.Id), default);

        // Assert
        result.Should().BeFailure()
            .HaveError(ErrorCodes.Forbidden);
        (await _orders.GetAsync(order.Id, default)).Should().BeSuccess()
            .HaveData(saved => saved!.IsOpen);
    }

    [Fact]
    public async Task HandleAsync_AlreadyCancelledOrder_ShouldReturnConflictAndNotRestockTwice()
    {
        // Arrange
        var order = await SaveOrderAsync("alice");
        await _sut.HandleAsync(new CancelOrderCommand(order.Id), default);

        // Act
        var result = await _sut.HandleAsync(new CancelOrderCommand(order.Id), default);

        // Assert
        result.Should().BeFailure()
            .HaveError(OrderErrors.Codes.AlreadyCancelled)
            .HaveType(ErrorType.Conflict);
        (await _books.GetAsync(TestData.CleanCodeIsbn, default)).Should().BeSuccess()
            .HaveData(book => book!.Stock == 10);
    }
}
