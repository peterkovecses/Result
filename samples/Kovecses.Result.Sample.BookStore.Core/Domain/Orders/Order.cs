namespace Kovecses.Result.Sample.BookStore.Core.Domain.Orders;

public enum OrderStatus
{
    Placed,
    Cancelled
}

public sealed record OrderLine(string Isbn, string Title, decimal UnitPrice, int Quantity)
{
    public decimal Total => UnitPrice * Quantity;
}

/// <summary>
/// A paid order. Orders are only created after the payment succeeded.
/// </summary>
public sealed record Order(
    Guid Id,
    string CustomerId,
    IReadOnlyList<OrderLine> Lines,
    string PaymentReference,
    DateTimeOffset PlacedAt,
    OrderStatus Status = OrderStatus.Placed)
{
    public decimal Total => Lines.Sum(line => line.Total);

    public bool IsOpen => Status == OrderStatus.Placed;

    public static Result<Order> Place(
        string customerId,
        IReadOnlyList<OrderLine> lines,
        string paymentReference,
        DateTimeOffset placedAt)
        => lines.Count == 0
            ? Result.Failure<Order>(OrderErrors.Codes.EmptyOrder, "An order needs at least one line.")
            : new Order(Guid.NewGuid(), customerId, lines, paymentReference, placedAt);

    public Result<Order> Cancel()
        => Result.Success(this)
            .Ensure(order => order.IsOpen, OrderErrors.AlreadyCancelled(Id))
            .Map(order => order with { Status = OrderStatus.Cancelled });
}
