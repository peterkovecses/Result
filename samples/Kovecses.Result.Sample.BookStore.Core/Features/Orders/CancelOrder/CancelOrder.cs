namespace Kovecses.Result.Sample.BookStore.Core.Features.Orders.CancelOrder;

public sealed record CancelOrderCommand(Guid OrderId) : IRequest<Result>;

public sealed class CancelOrderHandler(IOrderRepository orders, IBookRepository books, ICurrentUser currentUser)
    : IRequestHandler<CancelOrderCommand, Result>
{
    public async Task<Result> HandleAsync(CancelOrderCommand request, CancellationToken cancellationToken)
        => await currentUser.RequireUserId().Match(
            userId => CancelAsync(userId, request.OrderId, cancellationToken),
            errors => Task.FromResult<Result>(errors));

    private async Task<Result> CancelAsync(string userId, Guid orderId, CancellationToken cancellationToken)
    {
        var cancelled = await orders.GetAsync(orderId, cancellationToken)
            .EnsureAsync(order => order.CustomerId == userId, OrderErrors.NotOwner())
            .BindAsync(order => order.Cancel());

        return await cancelled.MatchFirstAsync(
            async order =>
            {
                await ReleaseStockAsync(order, cancellationToken);
                await orders.SaveAsync(order, cancellationToken);

                return Result.Success(new Dictionary<string, object> { ["refundAmount"] = order.Total });
            },
            error => Task.FromResult<Result>(error));
    }

    private Task ReleaseStockAsync(Order order, CancellationToken cancellationToken)
    {
        var releases = order.Lines
            .Select(line => new StockReservationRequest(line.Isbn, line.Quantity))
            .ToList();

        return books.ReleaseAsync(releases, cancellationToken);
    }
}
