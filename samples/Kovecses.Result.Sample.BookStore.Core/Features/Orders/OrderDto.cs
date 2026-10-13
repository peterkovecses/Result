namespace Kovecses.Result.Sample.BookStore.Core.Features.Orders;

public sealed record OrderLineDto(string Isbn, string Title, decimal UnitPrice, int Quantity, decimal Total);

public sealed record OrderDto(
    Guid Id,
    string CustomerId,
    string Status,
    IReadOnlyList<OrderLineDto> Lines,
    decimal Total,
    DateTimeOffset PlacedAt)
{
    public static OrderDto From(Order order)
        => new(
            order.Id,
            order.CustomerId,
            order.Status.ToString(),
            order.Lines.Select(line => new OrderLineDto(line.Isbn, line.Title, line.UnitPrice, line.Quantity, line.Total)).ToList(),
            order.Total,
            order.PlacedAt);
}
