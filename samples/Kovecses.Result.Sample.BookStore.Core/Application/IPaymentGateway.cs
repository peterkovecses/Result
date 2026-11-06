namespace Kovecses.Result.Sample.BookStore.Core.Application;

public sealed record PaymentRequest(string CustomerId, decimal Amount);

public sealed record PaymentReceipt(string Reference);

/// <summary>
/// Talks to the payment provider. A declined payment is returned as a failed result;
/// infrastructure problems (timeouts, cancellation, provider faults) surface as exceptions.
/// </summary>
public interface IPaymentGateway
{
    Task<Result<PaymentReceipt>> ChargeAsync(PaymentRequest request, CancellationToken cancellationToken);
}
