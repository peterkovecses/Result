namespace Kovecses.Result.Sample.BookStore.Core.Tests.Support;

public sealed class TestCurrentUser(string? userId = TestCurrentUser.DefaultUserId) : ICurrentUser
{
    public const string DefaultUserId = "alice";

    public string? UserId { get; set; } = userId;
}

public sealed class StubPaymentGateway : IPaymentGateway
{
    private Func<PaymentRequest, Task<Result<PaymentReceipt>>> _behavior =
        _ => Task.FromResult<Result<PaymentReceipt>>(new PaymentReceipt("pay_test"));

    public List<PaymentRequest> Charges { get; } = [];

    public StubPaymentGateway Throws(Exception exception)
    {
        _behavior = _ => throw exception;

        return this;
    }

    public StubPaymentGateway Declines(string reason)
    {
        _behavior = _ => Task.FromResult(Result.Failure<PaymentReceipt>(OrderErrors.Codes.PaymentDeclined, reason, ErrorType.Failure));

        return this;
    }

    public Task<Result<PaymentReceipt>> ChargeAsync(PaymentRequest request, CancellationToken cancellationToken)
    {
        Charges.Add(request);

        return _behavior(request);
    }
}

public static class TestData
{
    public const string CleanCodeIsbn = "9780132350884";
    public const string DddIsbn = "9780321125217";
    public const string UnknownIsbn = "9999999999999";

    public static Book CleanCode(int stock = 10) => Book.Create(CleanCodeIsbn, "Clean Code", "Robert C. Martin", 40m, stock).ValueOrThrow();

    public static Book Ddd(int stock = 5) => Book.Create(DddIsbn, "Domain-Driven Design", "Eric Evans", 55m, stock).ValueOrThrow();

    public static Order OrderFor(string customerId, params OrderLine[] lines)
        => Order.Place(customerId, lines, "pay_test", DateTimeOffset.UtcNow).ValueOrThrow();

    public static OrderLine LineOf(Book book, int quantity) => new(book.Isbn, book.Title, book.Price, quantity);
}
