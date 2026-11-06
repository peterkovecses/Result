using System.Net;
using System.Text.Json;
using Kovecses.Result.Sample.MinimalApi.Tests.Support;

namespace Kovecses.Result.Sample.MinimalApi.Tests;

public sealed class OrderEndpointTests(BookStoreApplicationFactory factory)
    : IClassFixture<BookStoreApplicationFactory>
{
    private readonly HttpClient _client = factory.CreateClient();

    [Fact]
    public async Task PlaceOrder_AuthenticatedCustomer_ReturnsCreatedOrder()
    {
        var isbn = await BookStoreTestData.AddBookAsync(_client, BookStoreTestData.NewBook());
        using var response = await BookStoreTestData.SendAsCustomerAsync(
            _client,
            HttpMethod.Post,
            "/api/orders",
            new PlaceOrderCommand([new OrderItem(isbn, 1)]),
            "customer-1");

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        Assert.NotNull(response.Headers.Location);
        var order = await response.Content.ReadFromJsonAsync<OrderDto>();
        Assert.NotNull(order);
        Assert.Equal("customer-1", order.CustomerId);
    }

    [Fact]
    public async Task PlaceOrder_AnonymousCustomer_MapsUnauthorizedError()
    {
        using var response = await _client.PostAsJsonAsync(
            "/api/orders",
            new PlaceOrderCommand([new OrderItem("9780132350884", 1)]));

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task PlaceOrder_DeclinedPayment_MapsCustomPaymentRequiredResponse()
    {
        var isbn = await BookStoreTestData.AddBookAsync(_client, BookStoreTestData.NewBook(price: 1_100m));
        using var response = await BookStoreTestData.SendAsCustomerAsync(
            _client,
            HttpMethod.Post,
            "/api/orders",
            new PlaceOrderCommand([new OrderItem(isbn, 1)]),
            "customer-1");

        Assert.Equal(HttpStatusCode.PaymentRequired, response.StatusCode);
    }

    [Fact]
    public async Task PlaceOrder_PaymentTimeout_MapsRequestTimeoutError()
    {
        var isbn = await BookStoreTestData.AddBookAsync(_client, BookStoreTestData.NewBook());
        using var response = await BookStoreTestData.SendAsCustomerAsync(
            _client,
            HttpMethod.Post,
            "/api/orders",
            new PlaceOrderCommand([new OrderItem(isbn, 1)]),
            "timeout");

        Assert.Equal(HttpStatusCode.RequestTimeout, response.StatusCode);
    }

    [Fact]
    public async Task PlaceOrder_CanceledPayment_MapsCanceledError()
    {
        var isbn = await BookStoreTestData.AddBookAsync(_client, BookStoreTestData.NewBook());
        using var response = await BookStoreTestData.SendAsCustomerAsync(
            _client,
            HttpMethod.Post,
            "/api/orders",
            new PlaceOrderCommand([new OrderItem(isbn, 1)]),
            "canceled");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task PlaceOrder_UnexpectedPaymentFailure_MapsInternalServerError()
    {
        var isbn = await BookStoreTestData.AddBookAsync(_client, BookStoreTestData.NewBook());
        using var response = await BookStoreTestData.SendAsCustomerAsync(
            _client,
            HttpMethod.Post,
            "/api/orders",
            new PlaceOrderCommand([new OrderItem(isbn, 1)]),
            "unexpected");

        Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);
    }

    [Fact]
    public async Task GetOrder_OwnedOrder_ReturnsOrder()
    {
        var isbn = await BookStoreTestData.AddBookAsync(_client, BookStoreTestData.NewBook());
        var order = await BookStoreTestData.PlaceOrderAsync(_client, "customer-1", isbn);
        using var response = await BookStoreTestData.SendAsCustomerAsync(
            _client,
            HttpMethod.Get,
            $"/api/orders/{order.Id}",
            null,
            "customer-1");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var returnedOrder = await response.Content.ReadFromJsonAsync<OrderDto>();
        Assert.NotNull(returnedOrder);
        Assert.Equal(order.Id, returnedOrder.Id);
    }

    [Fact]
    public async Task GetOrder_OrderBelongsToAnotherCustomer_MapsForbiddenError()
    {
        var isbn = await BookStoreTestData.AddBookAsync(_client, BookStoreTestData.NewBook());
        var order = await BookStoreTestData.PlaceOrderAsync(_client, "owner", isbn);
        using var response = await BookStoreTestData.SendAsCustomerAsync(
            _client,
            HttpMethod.Get,
            $"/api/orders/{order.Id}",
            null,
            "another-customer");

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task CancelOrder_OwnedOrder_ReturnsNoContent()
    {
        var isbn = await BookStoreTestData.AddBookAsync(_client, BookStoreTestData.NewBook());
        var order = await BookStoreTestData.PlaceOrderAsync(_client, "customer-1", isbn);
        using var response = await BookStoreTestData.SendAsCustomerAsync(
            _client,
            HttpMethod.Post,
            $"/api/orders/{order.Id}/cancel",
            null,
            "customer-1");

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }

    [Fact]
    public async Task CancelOrder_AnonymousCustomer_MapsUnauthorizedError()
    {
        using var response = await _client.PostAsync($"/api/orders/{Guid.NewGuid()}/cancel", null);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task GetOrderSummary_OwnedOrder_ReturnsSummary()
    {
        var isbn = await BookStoreTestData.AddBookAsync(_client, BookStoreTestData.NewBook());
        var order = await BookStoreTestData.PlaceOrderAsync(_client, "customer-1", isbn);
        using var response = await BookStoreTestData.SendAsCustomerAsync(
            _client,
            HttpMethod.Get,
            $"/api/orders/{order.Id}/summary",
            null,
            "customer-1");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var summary = await response.Content.ReadFromJsonAsync<OrderSummary>();
        Assert.NotNull(summary);
        Assert.Equal(order.Id, summary.OrderId);
    }

    [Fact]
    public async Task GetOrderSummary_OrderBelongsToAnotherCustomer_MapsForbiddenError()
    {
        var isbn = await BookStoreTestData.AddBookAsync(_client, BookStoreTestData.NewBook());
        var order = await BookStoreTestData.PlaceOrderAsync(_client, "owner", isbn);
        using var response = await BookStoreTestData.SendAsCustomerAsync(
            _client,
            HttpMethod.Get,
            $"/api/orders/{order.Id}/summary",
            null,
            "another-customer");

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }
}
