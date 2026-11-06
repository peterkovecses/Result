using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Kovecses.Result;
using Kovecses.Result.FluentAssertions;
using Kovecses.Result.Sample.BookStore.Core.Application;
using Kovecses.Result.Sample.BookStore.Core.Domain.Orders;
using Kovecses.Result.Sample.BookStore.Core.Features.Catalog;
using Kovecses.Result.Sample.BookStore.Core.Features.Catalog.AddBook;
using Kovecses.Result.Sample.BookStore.Core.Features.Catalog.RestockBooks;
using Kovecses.Result.Sample.BookStore.Core.Features.Orders;
using Kovecses.Result.Sample.BookStore.Core.Features.Orders.GetOrderSummary;
using Kovecses.Result.Sample.BookStore.Core.Features.Orders.PlaceOrder;
using Kovecses.Result.Sample.Controllers.Controllers;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Xunit;

namespace Kovecses.Result.Sample.Controllers.Tests;

public sealed class ControllersIntegrationTests : IAsyncLifetime
{
    private const string CustomerHeader = "X-Customer-Id";
    private const string CleanCodeIsbn = "9780132350884";

    private WebApplicationFactory<Program> _factory = null!;
    private HttpClient _client = null!;

    public Task InitializeAsync()
    {
        _factory = new WebApplicationFactory<Program>();
        _client = _factory.CreateClient();

        return Task.CompletedTask;
    }

    public Task DisposeAsync()
    {
        _client.Dispose();
        _factory.Dispose();

        return Task.CompletedTask;
    }

    [Fact]
    public async Task GetBook_Existing_ShouldReturnBook()
    {
        var response = await _client.GetAsync($"/books/{CleanCodeIsbn}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var book = await response.Content.ReadFromJsonAsync<BookDto>();
        Assert.NotNull(book);
        Assert.Equal("Clean Code", book.Title);
    }

    [Fact]
    public async Task GetBook_Missing_ShouldReturnNotFound()
    {
        var response = await _client.GetAsync("/books/9999999999999");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task SearchBooks_ValidQuery_ShouldReturnPage()
    {
        var response = await _client.GetAsync("/books?term=clean&page=1&pageSize=10");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var page = await response.Content.ReadFromJsonAsync<PagedResult<BookDto>>();
        Assert.NotNull(page);
        Assert.Equal(2, page.Items.Count);
    }

    [Fact]
    public async Task SearchBooks_InvalidPage_ShouldReturnValidationProblem()
    {
        var response = await _client.GetAsync("/books?page=0");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var problem = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.True(problem.GetProperty("errors").TryGetProperty("Page", out _));
    }

    [Fact]
    public async Task AddBook_ValidCommand_ShouldReturnCreatedBook()
    {
        var response = await _client.PostAsJsonAsync("/books", NewBookCommand());

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var book = await response.Content.ReadFromJsonAsync<BookDto>();
        Assert.NotNull(book);
        Assert.Equal("9780000000001", book.Isbn);
        Assert.NotNull(response.Headers.Location);
    }

    [Fact]
    public async Task AddBook_DuplicateIsbn_ShouldReturnConflict()
    {
        var response = await _client.PostAsJsonAsync("/books", NewBookCommand(CleanCodeIsbn));

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }

    [Fact]
    public async Task AddBook_InvalidFields_ShouldMapValidationMetadata()
    {
        var response = await _client.PostAsJsonAsync("/books", new AddBookCommand("", "", "", 0, -1));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);
        var problem = await response.Content.ReadFromJsonAsync<JsonElement>();
        var errors = problem.GetProperty("errors");
        Assert.True(errors.TryGetProperty("Isbn", out var isbnErrors));
        Assert.Contains("The ISBN is required.", isbnErrors.EnumerateArray().Select(error => error.GetString()));
        Assert.True(errors.TryGetProperty("Price", out _));
    }

    [Fact]
    public async Task ChangeBookPrice_ValidPrice_ShouldReturnUpdatedBook()
    {
        var response = await _client.PutAsJsonAsync($"/books/{CleanCodeIsbn}/price", new ChangeBookPriceRequest(45m));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var book = await response.Content.ReadFromJsonAsync<BookDto>();
        Assert.NotNull(book);
        Assert.Equal(45m, book.Price);
    }

    [Fact]
    public async Task ChangeBookPrice_NonPositivePrice_ShouldReturnValidationProblem()
    {
        var response = await _client.PutAsJsonAsync($"/books/{CleanCodeIsbn}/price", new ChangeBookPriceRequest(0));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task RestockBook_ValidItems_ShouldReturnCountHeader()
    {
        var command = new RestockBooksCommand([new RestockItem(CleanCodeIsbn, 3)]);

        var response = await _client.PostAsJsonAsync("/books/restock", command);

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        Assert.Equal("1", response.Headers.GetValues("X-Restocked-Books").Single());
    }

    [Fact]
    public async Task RestockBook_UnknownIsbn_ShouldReturnNotFound()
    {
        var command = new RestockBooksCommand([new RestockItem("9999999999999", 3)]);

        var response = await _client.PostAsJsonAsync("/books/restock", command);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task RetireBook_ExistingBook_ShouldReturnNoContent()
    {
        var response = await _client.DeleteAsync("/books/9780134494166");

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }

    [Fact]
    public async Task RetireBook_MissingBook_ShouldReturnNotFound()
    {
        var response = await _client.DeleteAsync("/books/9999999999999");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task RetireBook_WithOpenOrder_ShouldMapFailureType()
    {
        SetCustomer("alice");
        await CreateOrderAsync();

        var response = await _client.DeleteAsync($"/books/{CleanCodeIsbn}");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task PlaceOrder_ValidOrder_ShouldReturnCreatedOrder()
    {
        SetCustomer("alice");

        var response = await _client.PostAsJsonAsync("/orders", NewOrderCommand());

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var order = await response.Content.ReadFromJsonAsync<OrderDto>();
        Assert.NotNull(order);
        Assert.Equal("alice", order.CustomerId);
        Assert.NotNull(response.Headers.Location);
    }

    [Fact]
    public async Task PlaceOrder_AnonymousCustomer_ShouldReturnUnauthorized()
    {
        var response = await _client.PostAsJsonAsync("/orders", NewOrderCommand());

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task PlaceOrder_DeclinedPayment_ShouldReturnPaymentRequired()
    {
        SetCustomer("alice");
        var bookResponse = await _client.PostAsJsonAsync(
            "/books",
            new AddBookCommand("9780000000002", "Expensive Book", "Sample Author", 1_001m, 1));
        Assert.Equal(HttpStatusCode.Created, bookResponse.StatusCode);

        var response = await _client.PostAsJsonAsync("/orders", NewOrderCommand("9780000000002"));

        Assert.Equal(HttpStatusCode.PaymentRequired, response.StatusCode);
        var problem = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal("Payment declined", problem.GetProperty("title").GetString());
    }

    [Theory]
    [InlineData("timeout", HttpStatusCode.RequestTimeout)]
    [InlineData("canceled", HttpStatusCode.BadRequest)]
    [InlineData("unexpected", HttpStatusCode.InternalServerError)]
    public async Task PlaceOrder_GatewayError_ShouldMapErrorType(string errorName, HttpStatusCode expectedStatus)
    {
        using var factory = new PaymentErrorFactory(CreatePaymentError(errorName));
        using var client = factory.CreateClient();
        client.DefaultRequestHeaders.Add(CustomerHeader, "alice");

        var response = await client.PostAsJsonAsync("/orders", NewOrderCommand());

        Assert.Equal(expectedStatus, response.StatusCode);
    }

    [Fact]
    public async Task GetOrder_OwnedOrder_ShouldReturnOrder()
    {
        SetCustomer("alice");
        var orderId = await CreateOrderAsync();

        var response = await _client.GetAsync($"/orders/{orderId}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var order = await response.Content.ReadFromJsonAsync<OrderDto>();
        Assert.NotNull(order);
        Assert.Equal(orderId, order.Id);
    }

    [Fact]
    public async Task GetOrder_NotOwnedOrder_ShouldReturnForbidden()
    {
        SetCustomer("alice");
        var orderId = await CreateOrderAsync();
        SetCustomer("bob");

        var response = await _client.GetAsync($"/orders/{orderId}");

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task CancelOrder_OwnedOrder_ShouldReturnNoContent()
    {
        SetCustomer("alice");
        var orderId = await CreateOrderAsync();

        var response = await _client.PostAsync($"/orders/{orderId}/cancel", content: null);

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }

    [Fact]
    public async Task CancelOrder_MissingOrder_ShouldReturnNotFound()
    {
        SetCustomer("alice");

        var response = await _client.PostAsync($"/orders/{Guid.NewGuid()}/cancel", content: null);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task GetOrderSummary_OwnedOrder_ShouldReturnSummary()
    {
        SetCustomer("alice");
        var orderId = await CreateOrderAsync();

        var response = await _client.GetAsync($"/orders/{orderId}/summary");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var summary = await response.Content.ReadFromJsonAsync<OrderSummary>();
        Assert.NotNull(summary);
        Assert.Equal(orderId, summary.OrderId);
    }

    [Fact]
    public async Task GetOrderSummary_MissingOrder_ShouldReturnNotFound()
    {
        SetCustomer("alice");

        var response = await _client.GetAsync($"/orders/{Guid.NewGuid()}/summary");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task GetInternalOrder_ExistingOrder_ShouldReturnWrappedResult()
    {
        SetCustomer("alice");
        var orderId = await CreateOrderAsync();

        var response = await _client.GetAsync($"/internal/orders/{orderId}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<Result<OrderDto>>();
        Assert.NotNull(result);
        result.Should().BeSuccess();
        Assert.Equal(orderId, result.ValueOrThrow().Id);
    }

    [Fact]
    public async Task GetInternalOrder_MissingOrder_ShouldReturnWrappedFailure()
    {
        SetCustomer("alice");

        var response = await _client.GetAsync($"/internal/orders/{Guid.NewGuid()}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<Result<OrderDto>>();
        Assert.NotNull(result);
        result.Should().BeFailure()
            .HaveError(OrderErrors.Codes.NotFound);
    }

    private void SetCustomer(string? customerId)
    {
        _client.DefaultRequestHeaders.Remove(CustomerHeader);

        if (customerId is not null)
        {
            _client.DefaultRequestHeaders.Add(CustomerHeader, customerId);
        }
    }

    private async Task<Guid> CreateOrderAsync(string isbn = CleanCodeIsbn)
    {
        var response = await _client.PostAsJsonAsync("/orders", NewOrderCommand(isbn));
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var order = await response.Content.ReadFromJsonAsync<OrderDto>();
        Assert.NotNull(order);

        return order.Id;
    }

    private static AddBookCommand NewBookCommand(string isbn = "9780000000001")
        => new(isbn, "A Sample Book", "Sample Author", 12.50m, 4);

    private static PlaceOrderCommand NewOrderCommand(string isbn = CleanCodeIsbn)
        => new([new OrderItem(isbn, 1)]);

    private static Error CreatePaymentError(string errorName)
        => errorName switch
        {
            "timeout" => Error.Timeout(message: "Payment timed out."),
            "canceled" => Error.Canceled(message: "Payment was canceled."),
            "unexpected" => Error.Unexpected(message: "Payment gateway failed."),
            _ => throw new ArgumentOutOfRangeException(nameof(errorName), errorName, "Unknown payment error.")
        };

    private sealed class PaymentErrorFactory(Error error) : WebApplicationFactory<Program>
    {
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.ConfigureTestServices(services =>
            {
                services.RemoveAll<IPaymentGateway>();
                services.AddSingleton<IPaymentGateway>(new FixedPaymentGateway(error));
            });
        }
    }

    private sealed class FixedPaymentGateway(Error error) : IPaymentGateway
    {
        public Task<Result<PaymentReceipt>> ChargeAsync(PaymentRequest request, CancellationToken cancellationToken)
            => Task.FromResult(Result.Failure<PaymentReceipt>(error));
    }
}
