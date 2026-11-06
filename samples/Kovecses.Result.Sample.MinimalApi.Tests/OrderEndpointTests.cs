namespace Kovecses.Result.Sample.MinimalApi.Tests;

public sealed class OrderEndpointTests(BookStoreApplicationFactory factory)
    : IClassFixture<BookStoreApplicationFactory>
{
    private readonly HttpClient _client = factory.CreateClient();

    [Fact]
    public async Task PlaceOrder_WhenCustomerIsAuthenticated_ShouldReturnCreatedOrder()
    {
        // Arrange
        var isbn = await BookStoreTestData.AddBookAsync(_client, BookStoreTestData.NewBook());

        // Act
        using var response = await BookStoreTestData.SendAsCustomerAsync(
            _client,
            HttpMethod.Post,
            "/api/orders",
            new PlaceOrderCommand([new OrderItem(isbn, 1)]),
            "customer-1");

        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        Assert.NotNull(response.Headers.Location);
        var order = await response.Content.ReadFromJsonAsync<OrderDto>();
        Assert.NotNull(order);
        Assert.Equal("customer-1", order.CustomerId);
    }

    [Fact]
    public async Task PlaceOrder_WhenCustomerIsAnonymous_ShouldMapUnauthorizedError()
    {
        // Arrange
        var command = new PlaceOrderCommand([new OrderItem("9780132350884", 1)]);

        // Act
        using var response = await _client.PostAsJsonAsync(
            "/api/orders",
            command);

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task PlaceOrder_WhenPaymentIsDeclined_ShouldMapPaymentRequired()
    {
        // Arrange
        var isbn = await BookStoreTestData.AddBookAsync(_client, BookStoreTestData.NewBook(price: 1_100m));

        // Act
        using var response = await BookStoreTestData.SendAsCustomerAsync(
            _client,
            HttpMethod.Post,
            "/api/orders",
            new PlaceOrderCommand([new OrderItem(isbn, 1)]),
            "customer-1");

        // Assert
        Assert.Equal(HttpStatusCode.PaymentRequired, response.StatusCode);
    }

    [Fact]
    public async Task PlaceOrder_WhenPaymentTimesOut_ShouldMapRequestTimeoutError()
    {
        // Arrange
        var isbn = await BookStoreTestData.AddBookAsync(_client, BookStoreTestData.NewBook());

        // Act
        using var response = await BookStoreTestData.SendAsCustomerAsync(
            _client,
            HttpMethod.Post,
            "/api/orders",
            new PlaceOrderCommand([new OrderItem(isbn, 1)]),
            "timeout");

        // Assert
        Assert.Equal(HttpStatusCode.RequestTimeout, response.StatusCode);
    }

    [Fact]
    public async Task PlaceOrder_WhenPaymentIsCanceled_ShouldMapCanceledError()
    {
        // Arrange
        var isbn = await BookStoreTestData.AddBookAsync(_client, BookStoreTestData.NewBook());

        // Act
        using var response = await BookStoreTestData.SendAsCustomerAsync(
            _client,
            HttpMethod.Post,
            "/api/orders",
            new PlaceOrderCommand([new OrderItem(isbn, 1)]),
            "canceled");

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task PlaceOrder_WhenPaymentFailsUnexpectedly_ShouldMapInternalServerError()
    {
        // Arrange
        var isbn = await BookStoreTestData.AddBookAsync(_client, BookStoreTestData.NewBook());

        // Act
        using var response = await BookStoreTestData.SendAsCustomerAsync(
            _client,
            HttpMethod.Post,
            "/api/orders",
            new PlaceOrderCommand([new OrderItem(isbn, 1)]),
            "unexpected");

        // Assert
        Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);
    }

    [Fact]
    public async Task GetOrder_WhenOrderBelongsToCustomer_ShouldReturnOrder()
    {
        // Arrange
        var isbn = await BookStoreTestData.AddBookAsync(_client, BookStoreTestData.NewBook());
        var order = await BookStoreTestData.PlaceOrderAsync(_client, "customer-1", isbn);

        // Act
        using var response = await BookStoreTestData.SendAsCustomerAsync(
            _client,
            HttpMethod.Get,
            $"/api/orders/{order.Id}",
            null,
            "customer-1");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var returnedOrder = await response.Content.ReadFromJsonAsync<OrderDto>();
        Assert.NotNull(returnedOrder);
        Assert.Equal(order.Id, returnedOrder.Id);
    }

    [Fact]
    public async Task GetOrder_WhenOrderBelongsToAnotherCustomer_ShouldMapForbiddenError()
    {
        // Arrange
        var isbn = await BookStoreTestData.AddBookAsync(_client, BookStoreTestData.NewBook());
        var order = await BookStoreTestData.PlaceOrderAsync(_client, "owner", isbn);

        // Act
        using var response = await BookStoreTestData.SendAsCustomerAsync(
            _client,
            HttpMethod.Get,
            $"/api/orders/{order.Id}",
            null,
            "another-customer");

        // Assert
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task CancelOrder_WhenOrderBelongsToCustomer_ShouldReturnNoContent()
    {
        // Arrange
        var isbn = await BookStoreTestData.AddBookAsync(_client, BookStoreTestData.NewBook());
        var order = await BookStoreTestData.PlaceOrderAsync(_client, "customer-1", isbn);

        // Act
        using var response = await BookStoreTestData.SendAsCustomerAsync(
            _client,
            HttpMethod.Post,
            $"/api/orders/{order.Id}/cancel",
            null,
            "customer-1");

        // Assert
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }

    [Fact]
    public async Task CancelOrder_WhenCustomerIsAnonymous_ShouldMapUnauthorizedError()
    {
        // Arrange
        var requestUri = $"/api/orders/{Guid.NewGuid()}/cancel";

        // Act
        using var response = await _client.PostAsync(requestUri, null);

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task GetOrderSummary_WhenOrderBelongsToCustomer_ShouldReturnSummary()
    {
        // Arrange
        var isbn = await BookStoreTestData.AddBookAsync(_client, BookStoreTestData.NewBook());
        var order = await BookStoreTestData.PlaceOrderAsync(_client, "customer-1", isbn);

        // Act
        using var response = await BookStoreTestData.SendAsCustomerAsync(
            _client,
            HttpMethod.Get,
            $"/api/orders/{order.Id}/summary",
            null,
            "customer-1");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var summary = await response.Content.ReadFromJsonAsync<OrderSummary>();
        Assert.NotNull(summary);
        Assert.Equal(order.Id, summary.OrderId);
    }

    [Fact]
    public async Task GetOrderSummary_WhenOrderBelongsToAnotherCustomer_ShouldMapForbiddenError()
    {
        // Arrange
        var isbn = await BookStoreTestData.AddBookAsync(_client, BookStoreTestData.NewBook());
        var order = await BookStoreTestData.PlaceOrderAsync(_client, "owner", isbn);

        // Act
        using var response = await BookStoreTestData.SendAsCustomerAsync(
            _client,
            HttpMethod.Get,
            $"/api/orders/{order.Id}/summary",
            null,
            "another-customer");

        // Assert
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }
}
