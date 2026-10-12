namespace Kovecses.Result.Sample.BookStore.Core.Domain.Orders;

public interface IOrderRepository
{
    /// <summary>Gets an order by id, or a NotFound failure.</summary>
    Task<Result<Order>> GetAsync(Guid orderId, CancellationToken cancellationToken);

    Task<bool> HasOpenOrdersForAsync(string isbn, CancellationToken cancellationToken);

    Task SaveAsync(Order order, CancellationToken cancellationToken);
}
