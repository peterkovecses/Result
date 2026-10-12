using Kovecses.Result.Sample.BookStore.Core.Features.Orders.GetOrder;
using Kovecses.Result.Sample.BookStore.Core.Features.Orders.GetOrderSummary;

namespace Kovecses.Result.Sample.BookStore.Core.Tests.Features.Orders;

public class GetOrderSummaryHandlerTests
{
    private readonly InMemoryBookRepository _books = new([TestData.CleanCode(stock: 10)]);
    private readonly InMemoryOrderRepository _orders = new();
    private readonly TestCurrentUser _user = new();
    private readonly GetOrderSummaryHandler _sut;

    public GetOrderSummaryHandlerTests()
        => _sut = new GetOrderSummaryHandler(new GetOrderHandler(_orders, _user), _books);

    private async Task<Order> SaveOrderAsync(string customerId, int quantity = 2)
    {
        var order = TestData.OrderFor(customerId, TestData.LineOf(TestData.CleanCode(), quantity));
        await _orders.SaveAsync(order, default);

        return order;
    }

    [Fact]
    public async Task HandleAsync_OwnOrder_ShouldDescribeOrderAndAllowReorder()
    {
        // Arrange
        var order = await SaveOrderAsync("alice");

        // Act
        var result = await _sut.HandleAsync(new GetOrderSummaryQuery(order.Id), default);

        // Assert
        result.Should().BeSuccess()
            .HaveData(summary => summary!.Text == "Placed order of 2 x Clean Code for 80.00." && summary.CanReorder);
    }

    [Fact]
    public async Task HandleAsync_BookRanOutOfStock_ShouldNotAllowReorder()
    {
        // Arrange
        var order = await SaveOrderAsync("alice", quantity: 3);
        await _books.SaveAsync(TestData.CleanCode(stock: 1), default);

        // Act
        var result = await _sut.HandleAsync(new GetOrderSummaryQuery(order.Id), default);

        // Assert
        result.Should().BeSuccess()
            .HaveData(summary => !summary!.CanReorder);
    }

    [Fact]
    public async Task HandleAsync_AnonymousUser_ShouldReturnUnauthorized()
    {
        // Arrange
        _user.UserId = null;

        // Act
        var result = await _sut.HandleAsync(new GetOrderSummaryQuery(Guid.NewGuid()), default);

        // Assert
        result.Should().BeFailure()
            .HaveError(ErrorCodes.Unauthorized);
    }

    [Fact]
    public async Task HandleAsync_UnknownOrder_ShouldReturnNotFound()
    {
        // Arrange
        var query = new GetOrderSummaryQuery(Guid.NewGuid());

        // Act
        var result = await _sut.HandleAsync(query, default);

        // Assert
        result.Should().BeFailure()
            .HaveError(OrderErrors.Codes.NotFound);
    }

    [Fact]
    public async Task HandleAsync_OrderOfAnotherCustomer_ShouldReturnForbidden()
    {
        // Arrange
        var order = await SaveOrderAsync("bob");

        // Act
        var result = await _sut.HandleAsync(new GetOrderSummaryQuery(order.Id), default);

        // Assert
        result.Should().BeFailure()
            .HaveError(ErrorCodes.Forbidden);
    }
}
