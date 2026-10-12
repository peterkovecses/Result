using Kovecses.Result.Sample.BookStore.Core.Features.Orders.GetOrder;

namespace Kovecses.Result.Sample.BookStore.Core.Tests.Features.Orders;

public class GetOrderHandlerTests
{
    private readonly InMemoryOrderRepository _orders = new();
    private readonly TestCurrentUser _user = new();
    private readonly GetOrderHandler _sut;

    public GetOrderHandlerTests() => _sut = new GetOrderHandler(_orders, _user);

    [Fact]
    public async Task HandleAsync_OwnOrder_ShouldReturnOrder()
    {
        // Arrange
        var order = TestData.OrderFor("alice", TestData.LineOf(TestData.CleanCode(), 2));
        await _orders.SaveAsync(order, default);

        // Act
        var result = await _sut.HandleAsync(new GetOrderQuery(order.Id), default);

        // Assert
        result.Should().BeSuccess()
            .HaveData(dto => dto!.Id == order.Id && dto.Total == 80m);
    }

    [Fact]
    public async Task HandleAsync_AnonymousUser_ShouldReturnUnauthorized()
    {
        // Arrange
        _user.UserId = null;

        // Act
        var result = await _sut.HandleAsync(new GetOrderQuery(Guid.NewGuid()), default);

        // Assert
        result.Should().BeFailure()
            .HaveError(ErrorCodes.Unauthorized);
    }

    [Fact]
    public async Task HandleAsync_UnknownOrder_ShouldReturnNotFound()
    {
        // Arrange
        var query = new GetOrderQuery(Guid.NewGuid());

        // Act
        var result = await _sut.HandleAsync(query, default);

        // Assert
        result.Should().BeFailure()
            .HaveError(OrderErrors.Codes.NotFound)
            .HaveType(ErrorType.NotFound);
    }

    [Fact]
    public async Task HandleAsync_OrderOfAnotherCustomer_ShouldReturnForbidden()
    {
        // Arrange
        var order = TestData.OrderFor("bob", TestData.LineOf(TestData.CleanCode(), 1));
        await _orders.SaveAsync(order, default);

        // Act
        var result = await _sut.HandleAsync(new GetOrderQuery(order.Id), default);

        // Assert
        result.Should().BeFailure()
            .HaveError(ErrorCodes.Forbidden)
            .HaveType(ErrorType.Forbidden);
    }
}
