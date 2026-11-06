namespace Kovecses.Result.Sample.BookStore.Core.Infrastructure;

/// <summary>
/// A stand-in for a real payment provider: approves everything up to a per-order limit.
/// </summary>
public sealed class FakePaymentGateway : IPaymentGateway
{
    public const decimal PerOrderLimit = 1_000m;

    public Task<Result<PaymentReceipt>> ChargeAsync(PaymentRequest request, CancellationToken cancellationToken)
    {
        Result<PaymentReceipt> result = request.Amount > PerOrderLimit
            ? Result.Failure<PaymentReceipt>(
                OrderErrors.Codes.PaymentDeclined,
                $"The payment of {request.Amount:0.00} exceeds the limit of {PerOrderLimit:0.00}.",
                ErrorType.Failure)
            : new PaymentReceipt($"pay_{Guid.NewGuid():N}");

        return Task.FromResult(result);
    }
}
