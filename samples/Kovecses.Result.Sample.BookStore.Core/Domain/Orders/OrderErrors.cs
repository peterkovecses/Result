namespace Kovecses.Result.Sample.BookStore.Core.Domain.Orders;

public static class OrderErrors
{
    public static class Codes
    {
        public const string NotFound = "Orders.OrderNotFound";
        public const string AlreadyCancelled = "Orders.AlreadyCancelled";
        public const string EmptyOrder = "Orders.EmptyOrder";
        public const string PaymentDeclined = "Orders.PaymentDeclined";
    }

    public static Error NotFound(Guid orderId)
        => Error.NotFound(message: $"Order '{orderId}' was not found.", code: Codes.NotFound);

    public static Error NotOwner()
        => Error.Forbidden(message: "This order belongs to another customer.");

    public static Error AlreadyCancelled(Guid orderId)
        => Error.Conflict(message: $"Order '{orderId}' has already been cancelled.", code: Codes.AlreadyCancelled);

    public static Error PaymentCanceled()
        => Error.Canceled(message: "The payment was canceled before it completed.");

    public static Error PaymentTimedOut()
        => Error.Timeout(message: "The payment gateway did not respond in time.");

    public static Error PaymentGatewayFailed()
        => Error.Unexpected(message: "The payment could not be processed because of a gateway error.");
}
