namespace Kovecses.Result.Sample.BookStore.Core.Features.Orders.GetOrder;

public sealed record GetOrderQuery(Guid OrderId) : IRequest<Result<OrderDto>>;

public sealed class GetOrderHandler(IOrderRepository orders, ICurrentUser currentUser)
    : IRequestHandler<GetOrderQuery, Result<OrderDto>>
{
    public async Task<Result<OrderDto>> HandleAsync(GetOrderQuery request, CancellationToken cancellationToken)
        => await currentUser.RequireUserId().BindAsync(userId =>
            orders.GetAsync(request.OrderId, cancellationToken)
                .EnsureAsync(order => order.CustomerId == userId, OrderErrors.NotOwner())
                .MapAsync(order => OrderDto.From(order)));
}
