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
    public async Task GetBook_WhenBookExists_ShouldReturnBook()
    {
        // Arrange
        var uri = $"/books/{CleanCodeIsbn}";

        // Act
        var response = await _client.GetAsync(uri);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var book = await response.Content.ReadFromJsonAsync<BookDto>();
        Assert.NotNull(book);
        Assert.Equal("Clean Code", book.Title);
    }

    [Fact]
    public async Task GetBook_WhenBookIsMissing_ShouldReturnNotFound()
    {
        // Arrange
        const string uri = "/books/9999999999999";

        // Act
        var response = await _client.GetAsync(uri);

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task SearchBooks_WhenQueryIsValid_ShouldReturnPage()
    {
        // Arrange
        const string uri = "/books?term=clean&page=1&pageSize=10";

        // Act
        var response = await _client.GetAsync(uri);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var page = await response.Content.ReadFromJsonAsync<PagedResult<BookDto>>();
        Assert.NotNull(page);
        Assert.Equal(2, page.Items.Count);
    }

    [Fact]
    public async Task SearchBooks_WhenPageIsInvalid_ShouldReturnValidationProblem()
    {
        // Arrange
        const string uri = "/books?page=0";

        // Act
        var response = await _client.GetAsync(uri);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var problem = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.True(problem.GetProperty("errors").TryGetProperty("Page", out _));
    }

    [Fact]
    public async Task AddBook_WhenCommandIsValid_ShouldReturnCreatedBook()
    {
        // Arrange
        var command = NewBookCommand();

        // Act
        var response = await _client.PostAsJsonAsync("/books", command);

        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var book = await response.Content.ReadFromJsonAsync<BookDto>();
        Assert.NotNull(book);
        Assert.Equal("9780000000001", book.Isbn);
        Assert.NotNull(response.Headers.Location);
    }

    [Fact]
    public async Task AddBook_WhenIsbnAlreadyExists_ShouldReturnConflict()
    {
        // Arrange
        var command = NewBookCommand(CleanCodeIsbn);

        // Act
        var response = await _client.PostAsJsonAsync("/books", command);

        // Assert
        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }

    [Fact]
    public async Task AddBook_WhenFieldsAreInvalid_ShouldMapValidationMetadata()
    {
        // Arrange
        var command = new AddBookCommand("", "", "", 0, -1);

        // Act
        var response = await _client.PostAsJsonAsync("/books", command);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);
        var problem = await response.Content.ReadFromJsonAsync<JsonElement>();
        var errors = problem.GetProperty("errors");
        Assert.True(errors.TryGetProperty("Isbn", out var isbnErrors));
        Assert.Contains("The ISBN is required.", isbnErrors.EnumerateArray().Select(error => error.GetString()));
        Assert.True(errors.TryGetProperty("Price", out _));
    }

    [Fact]
    public async Task ChangeBookPrice_WhenPriceIsValid_ShouldReturnUpdatedBook()
    {
        // Arrange
        var request = new ChangeBookPriceRequest(45m);

        // Act
        var response = await _client.PutAsJsonAsync($"/books/{CleanCodeIsbn}/price", request);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var book = await response.Content.ReadFromJsonAsync<BookDto>();
        Assert.NotNull(book);
        Assert.Equal(45m, book.Price);
    }

    [Fact]
    public async Task ChangeBookPrice_WhenPriceIsNonPositive_ShouldReturnValidationProblem()
    {
        // Arrange
        var request = new ChangeBookPriceRequest(0);

        // Act
        var response = await _client.PutAsJsonAsync($"/books/{CleanCodeIsbn}/price", request);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task RestockBook_WhenItemsAreValid_ShouldReturnCountHeader()
    {
        // Arrange
        var command = new RestockBooksCommand([new RestockItem(CleanCodeIsbn, 3)]);

        // Act
        var response = await _client.PostAsJsonAsync("/books/restock", command);

        // Assert
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        Assert.Equal("1", response.Headers.GetValues("X-Restocked-Books").Single());
    }

    [Fact]
    public async Task RestockBook_WhenIsbnIsUnknown_ShouldReturnNotFound()
    {
        // Arrange
        var command = new RestockBooksCommand([new RestockItem("9999999999999", 3)]);

        // Act
        var response = await _client.PostAsJsonAsync("/books/restock", command);

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task RetireBook_WhenBookExists_ShouldReturnNoContent()
    {
        // Arrange
        const string uri = "/books/9780134494166";

        // Act
        var response = await _client.DeleteAsync(uri);

        // Assert
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }

    [Fact]
    public async Task RetireBook_WhenBookIsMissing_ShouldReturnNotFound()
    {
        // Arrange
        const string uri = "/books/9999999999999";

        // Act
        var response = await _client.DeleteAsync(uri);

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task RetireBook_WhenBookHasOpenOrder_ShouldMapFailureType()
    {
        // Arrange
        SetCustomer("alice");
        await CreateOrderAsync();

        // Act
        var response = await _client.DeleteAsync($"/books/{CleanCodeIsbn}");

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task PlaceOrder_WhenOrderIsValid_ShouldReturnCreatedOrder()
    {
        // Arrange
        SetCustomer("alice");
        var command = NewOrderCommand();

        // Act
        var response = await _client.PostAsJsonAsync("/orders", command);

        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var order = await response.Content.ReadFromJsonAsync<OrderDto>();
        Assert.NotNull(order);
        Assert.Equal("alice", order.CustomerId);
        Assert.NotNull(response.Headers.Location);
    }

    [Fact]
    public async Task PlaceOrder_WhenCustomerIsAnonymous_ShouldReturnUnauthorized()
    {
        // Arrange
        var command = NewOrderCommand();

        // Act
        var response = await _client.PostAsJsonAsync("/orders", command);

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task PlaceOrder_WhenPaymentIsDeclined_ShouldReturnPaymentRequired()
    {
        // Arrange
        SetCustomer("alice");
        var bookResponse = await _client.PostAsJsonAsync(
            "/books",
            new AddBookCommand("9780000000002", "Expensive Book", "Sample Author", 1_001m, 1));
        Assert.Equal(HttpStatusCode.Created, bookResponse.StatusCode);

        var command = NewOrderCommand("9780000000002");

        // Act
        var response = await _client.PostAsJsonAsync("/orders", command);

        // Assert
        Assert.Equal(HttpStatusCode.PaymentRequired, response.StatusCode);
        var problem = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal("Payment declined", problem.GetProperty("title").GetString());
    }

    [Theory]
    [InlineData("timeout", HttpStatusCode.RequestTimeout)]
    [InlineData("canceled", HttpStatusCode.BadRequest)]
    [InlineData("unexpected", HttpStatusCode.InternalServerError)]
    public async Task PlaceOrder_WhenGatewayReturnsError_ShouldMapErrorType(string errorName, HttpStatusCode expectedStatus)
    {
        // Arrange
        using var factory = new PaymentErrorFactory(CreatePaymentError(errorName));
        using var client = factory.CreateClient();
        client.DefaultRequestHeaders.Add(CustomerHeader, "alice");
        var command = NewOrderCommand();

        // Act
        var response = await client.PostAsJsonAsync("/orders", command);

        // Assert
        Assert.Equal(expectedStatus, response.StatusCode);
    }

    [Fact]
    public async Task GetOrder_WhenOrderIsOwnedByCustomer_ShouldReturnOrder()
    {
        // Arrange
        SetCustomer("alice");
        var orderId = await CreateOrderAsync();

        // Act
        var response = await _client.GetAsync($"/orders/{orderId}");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var order = await response.Content.ReadFromJsonAsync<OrderDto>();
        Assert.NotNull(order);
        Assert.Equal(orderId, order.Id);
    }

    [Fact]
    public async Task GetOrder_WhenOrderIsOwnedByAnotherCustomer_ShouldReturnForbidden()
    {
        // Arrange
        SetCustomer("alice");
        var orderId = await CreateOrderAsync();
        SetCustomer("bob");

        // Act
        var response = await _client.GetAsync($"/orders/{orderId}");

        // Assert
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task CancelOrder_WhenOrderIsOwnedByCustomer_ShouldReturnNoContent()
    {
        // Arrange
        SetCustomer("alice");
        var orderId = await CreateOrderAsync();

        // Act
        var response = await _client.PostAsync($"/orders/{orderId}/cancel", content: null);

        // Assert
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }

    [Fact]
    public async Task CancelOrder_WhenOrderIsMissing_ShouldReturnNotFound()
    {
        // Arrange
        SetCustomer("alice");
        var orderId = Guid.NewGuid();

        // Act
        var response = await _client.PostAsync($"/orders/{orderId}/cancel", content: null);

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task GetOrderSummary_WhenOrderIsOwnedByCustomer_ShouldReturnSummary()
    {
        // Arrange
        SetCustomer("alice");
        var orderId = await CreateOrderAsync();

        // Act
        var response = await _client.GetAsync($"/orders/{orderId}/summary");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var summary = await response.Content.ReadFromJsonAsync<OrderSummary>();
        Assert.NotNull(summary);
        Assert.Equal(orderId, summary.OrderId);
    }

    [Fact]
    public async Task GetOrderSummary_WhenOrderIsMissing_ShouldReturnNotFound()
    {
        // Arrange
        SetCustomer("alice");
        var orderId = Guid.NewGuid();

        // Act
        var response = await _client.GetAsync($"/orders/{orderId}/summary");

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task GetInternalOrder_WhenOrderExists_ShouldReturnWrappedResult()
    {
        // Arrange
        SetCustomer("alice");
        var orderId = await CreateOrderAsync();

        // Act
        var response = await _client.GetAsync($"/internal/orders/{orderId}");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<Result<OrderDto>>();
        Assert.NotNull(result);
        result.Should().BeSuccess();
        Assert.Equal(orderId, result.ValueOrThrow().Id);
    }

    [Fact]
    public async Task GetInternalOrder_WhenOrderIsMissing_ShouldReturnWrappedFailure()
    {
        // Arrange
        SetCustomer("alice");
        var orderId = Guid.NewGuid();

        // Act
        var response = await _client.GetAsync($"/internal/orders/{orderId}");

        // Assert
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
