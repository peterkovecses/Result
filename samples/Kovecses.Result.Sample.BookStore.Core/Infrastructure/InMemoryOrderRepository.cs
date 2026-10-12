using System.Collections.Concurrent;

namespace Kovecses.Result.Sample.BookStore.Core.Infrastructure;

public sealed class InMemoryOrderRepository : IOrderRepository
{
    private readonly ConcurrentDictionary<Guid, Order> _orders = new();

    public Task<Result<Order>> GetAsync(Guid orderId, CancellationToken cancellationToken)
        => Task.FromResult<Result<Order>>(
            _orders.TryGetValue(orderId, out var order) ? order : OrderErrors.NotFound(orderId));

    public Task<bool> HasOpenOrdersForAsync(string isbn, CancellationToken cancellationToken)
        => Task.FromResult(_orders.Values.Any(order =>
            order.IsOpen && order.Lines.Any(line => string.Equals(line.Isbn, isbn, StringComparison.OrdinalIgnoreCase))));

    public Task SaveAsync(Order order, CancellationToken cancellationToken)
    {
        _orders[order.Id] = order;

        return Task.CompletedTask;
    }
}
