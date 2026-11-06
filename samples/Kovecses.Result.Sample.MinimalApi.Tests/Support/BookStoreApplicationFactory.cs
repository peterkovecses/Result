namespace Kovecses.Result.Sample.MinimalApi.Tests.Support;

public sealed class BookStoreApplicationFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureTestServices(services =>
        {
            services.RemoveAll<IPaymentGateway>();
            services.AddSingleton<IPaymentGateway, TestPaymentGateway>();
        });
    }
}

internal sealed class TestPaymentGateway : IPaymentGateway
{
    public Task<Result<PaymentReceipt>> ChargeAsync(PaymentRequest request, CancellationToken cancellationToken)
    {
        if (request.CustomerId == "timeout")
        {
            throw new TimeoutException();
        }

        if (request.CustomerId == "unexpected")
        {
            throw new InvalidOperationException("Simulated payment gateway failure.");
        }

        if (request.CustomerId == "canceled")
        {
            return Task.FromResult<Result<PaymentReceipt>>(OrderErrors.PaymentCanceled());
        }

        Result<PaymentReceipt> result = request.Amount > FakePaymentGateway.PerOrderLimit
            ? Result.Failure<PaymentReceipt>(
                OrderErrors.Codes.PaymentDeclined,
                $"The payment of {request.Amount:0.00} exceeds the limit of {FakePaymentGateway.PerOrderLimit:0.00}.",
                ErrorType.Failure)
            : new PaymentReceipt($"pay_{Guid.NewGuid():N}");

        return Task.FromResult(result);
    }
}

internal static class BookStoreTestData
{
    public static string NewIsbn() => $"978{Random.Shared.NextInt64(0, 10_000_000_000):D10}";

    public static AddBookCommand NewBook(string? isbn = null, decimal price = 25m, int stock = 10)
        => new(isbn ?? NewIsbn(), "Integration Test Book", "Copilot Tests", price, stock);

    public static async Task<string> AddBookAsync(HttpClient client, AddBookCommand command)
    {
        using var response = await client.PostAsJsonAsync("/api/catalog/books", command);
        response.EnsureSuccessStatusCode();
        return command.Isbn;
    }

    public static async Task<OrderDto> PlaceOrderAsync(HttpClient client, string customerId, string isbn)
    {
        using var response = await SendAsCustomerAsync(
            client,
            HttpMethod.Post,
            "/api/orders",
            new PlaceOrderCommand([new OrderItem(isbn, 1)]),
            customerId);
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<OrderDto>())!;
    }

    public static Task<HttpResponseMessage> SendAsCustomerAsync(
        HttpClient client,
        HttpMethod method,
        string uri,
        object? body,
        string customerId)
    {
        var request = new HttpRequestMessage(method, uri);
        request.Headers.Add("X-Customer-Id", customerId);
        if (body is not null)
        {
            request.Content = JsonContent.Create(body);
        }

        return client.SendAsync(request);
    }
}
