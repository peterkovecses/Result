using Kovecses.Result.Sample.BookStore.Core.Features.Orders.PlaceOrder;

namespace Kovecses.Result.Sample.BookStore.Core.Tests.Features.Orders;

public class PlaceOrderHandlerTests
{
    private readonly InMemoryBookRepository _books = new([TestData.CleanCode(), TestData.Ddd(stock: 1)]);
    private readonly InMemoryOrderRepository _orders = new();
    private readonly StubPaymentGateway _gateway = new();
    private readonly TestCurrentUser _user = new();
    private readonly PlaceOrderHandler _sut;

    public PlaceOrderHandlerTests()
        => _sut = new PlaceOrderHandler(_books, _orders, _gateway, _user, TimeProvider.System, NullLogger<PlaceOrderHandler>.Instance);

    private static PlaceOrderCommand Order(OrderItem first) => new([first]);

    private static PlaceOrderCommand Order(OrderItem first, OrderItem second) => new([first, second]);

    private static PlaceOrderCommand Order(OrderItem first, OrderItem second, OrderItem third) => new([first, second, third]);

    [Fact]
    public async Task HandleAsync_AvailableBooks_ShouldPlaceOrderChargeAndReserveStock()
    {
        // Arrange
        var command = Order(new(TestData.CleanCodeIsbn, 2), new(TestData.DddIsbn, 1));

        // Act
        var result = await _sut.HandleAsync(command, default);

        // Assert
        result.Should().BeSuccess()
            .HaveData(order => order!.CustomerId == "alice" && order.Total == 135m && order.Lines.Count == 2);
        Assert.Equal(135m, _gateway.Charges.Single().Amount);
        (await _books.GetAsync(TestData.CleanCodeIsbn, default)).Should().BeSuccess()
            .HaveData(book => book!.Stock == 8);
        (await _books.GetAsync(TestData.DddIsbn, default)).Should().BeSuccess()
            .HaveData(book => book!.Stock == 0);
    }

    [Fact]
    public async Task HandleAsync_AnonymousUser_ShouldReturnUnauthorized()
    {
        // Arrange
        _user.UserId = null;
        var command = Order(new(TestData.CleanCodeIsbn, 1));

        // Act
        var result = await _sut.HandleAsync(command, default);

        // Assert
        result.Should().BeFailure()
            .HaveError(ErrorCodes.Unauthorized)
            .HaveType(ErrorType.Unauthorized);
        Assert.Empty(_gateway.Charges);
    }

    [Fact]
    public async Task HandleAsync_UnknownBooks_ShouldReturnNotFoundForEachMissingBook()
    {
        // Arrange
        var command = Order(new(TestData.UnknownIsbn, 1), new(TestData.CleanCodeIsbn, 1), new("1111111111111", 1));

        // Act
        var result = await _sut.HandleAsync(command, default);

        // Assert
        result.Should().BeFailure()
            .HaveErrors().HaveCount(2).AllBeOfType(ErrorType.NotFound);
        Assert.Empty(_gateway.Charges);
    }

    [Fact]
    public async Task HandleAsync_NotEnoughStock_ShouldReturnConflictWithStockMetadata()
    {
        // Arrange
        var command = Order(new(TestData.CleanCodeIsbn, 1), new(TestData.DddIsbn, 3));

        // Act
        var result = await _sut.HandleAsync(command, default);

        // Assert
        result.Should().BeFailure()
            .HaveError(BookErrors.Codes.InsufficientStock)
            .HaveType(ErrorType.Conflict)
            .HaveMetadata("available", 1);
        Assert.Empty(_gateway.Charges);
        (await _books.GetAsync(TestData.CleanCodeIsbn, default)).Should().BeSuccess()
            .HaveData(book => book!.Stock == 10);
    }

    [Fact]
    public async Task HandleAsync_RetiredBook_ShouldReturnConflict()
    {
        // Arrange
        await _books.SaveAsync(TestData.CleanCode() with { IsRetired = true }, default);
        var command = Order(new(TestData.CleanCodeIsbn, 1));

        // Act
        var result = await _sut.HandleAsync(command, default);

        // Assert
        result.Should().BeFailure()
            .HaveError(BookErrors.Codes.Retired)
            .HaveType(ErrorType.Conflict);
    }

    [Fact]
    public async Task HandleAsync_PaymentDeclined_ShouldReturnFailureAndKeepStock()
    {
        // Arrange
        _gateway.Declines("Card declined.");
        var command = Order(new(TestData.CleanCodeIsbn, 2));

        // Act
        var result = await _sut.HandleAsync(command, default);

        // Assert
        result.Should().BeFailure()
            .HaveError(OrderErrors.Codes.PaymentDeclined)
            .HaveType(ErrorType.Failure)
            .HaveMessage("Card declined.");
        (await _books.GetAsync(TestData.CleanCodeIsbn, default)).Should().BeSuccess()
            .HaveData(book => book!.Stock == 10);
    }

    [Fact]
    public async Task HandleAsync_GatewayTimesOut_ShouldReturnTimeout()
    {
        // Arrange
        _gateway.Throws(new TimeoutException());
        var command = Order(new(TestData.CleanCodeIsbn, 1));

        // Act
        var result = await _sut.HandleAsync(command, default);

        // Assert
        result.Should().BeFailure()
            .HaveError(ErrorCodes.Timeout)
            .HaveType(ErrorType.Timeout);
    }

    [Fact]
    public async Task HandleAsync_GatewayCallIsCanceledByTheCaller_ShouldReturnCanceled()
    {
        // Arrange
        _gateway.Throws(new OperationCanceledException());
        var command = Order(new(TestData.CleanCodeIsbn, 1));

        // Act
        var result = await _sut.HandleAsync(command, new CancellationToken(canceled: true));

        // Assert
        result.Should().BeFailure()
            .HaveError(ErrorCodes.Canceled)
            .HaveType(ErrorType.Canceled);
    }

    [Fact]
    public async Task HandleAsync_GatewayCancelsItself_ShouldReturnTimeout()
    {
        // Arrange
        _gateway.Throws(new TaskCanceledException());
        var command = Order(new(TestData.CleanCodeIsbn, 1));

        // Act
        var result = await _sut.HandleAsync(command, default);

        // Assert
        result.Should().BeFailure()
            .HaveError(ErrorCodes.Timeout);
    }

    [Fact]
    public async Task HandleAsync_GatewayFaults_ShouldReturnUnexpectedAndKeepStock()
    {
        // Arrange
        _gateway.Throws(new InvalidOperationException("boom"));
        var command = Order(new(TestData.CleanCodeIsbn, 1));

        // Act
        var result = await _sut.HandleAsync(command, default);

        // Assert
        result.Should().BeFailure()
            .HaveError(ErrorCodes.Unexpected)
            .HaveType(ErrorType.Unexpected);
        (await _books.GetAsync(TestData.CleanCodeIsbn, default)).Should().BeSuccess()
            .HaveData(book => book!.Stock == 10);
    }
}

public class PlaceOrderValidatorTests
{
    private readonly PlaceOrderValidator _sut = new();

    [Fact]
    public async Task ValidateAsync_ValidLines_ShouldSucceed()
    {
        // Arrange
        var command = new PlaceOrderCommand([new(TestData.CleanCodeIsbn, 2)]);

        // Act
        var result = await _sut.ValidateAsync(command, default);

        // Assert
        result.Should().BeSuccess();
    }

    [Fact]
    public async Task ValidateAsync_NoLines_ShouldReturnFieldError()
    {
        // Arrange
        var command = new PlaceOrderCommand([]);

        // Act
        var result = await _sut.ValidateAsync(command, default);

        // Assert
        result.Should().BeFailure()
            .HaveError(ErrorCodes.Validation)
            .HaveValidationProperty("Lines").Contain("at least one line");
    }

    [Fact]
    public async Task ValidateAsync_BadLines_ShouldReturnAllMessages()
    {
        // Arrange
        var command = new PlaceOrderCommand([new("", 1), new(TestData.CleanCodeIsbn, 21), new(TestData.CleanCodeIsbn, 1)]);

        // Act
        var result = await _sut.ValidateAsync(command, default);

        // Assert
        result.Should().BeFailure()
            .HaveError(ErrorCodes.Validation)
            .HaveValidationProperty("Lines").ContainAll("ISBN", "between 1 and 20", "only appear once");
    }
}
