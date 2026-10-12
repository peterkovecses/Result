using Kovecses.Result.Sample.BookStore.Core.Features.Orders.GetOrder;

namespace Kovecses.Result.Sample.BookStore.Core.Features.Orders.GetOrderSummary;

public sealed record GetOrderSummaryQuery(Guid OrderId) : IRequest<Result<OrderSummary>>;

public sealed record OrderSummary(Guid OrderId, string Text, bool CanReorder);

/// <summary>
/// Builds on the ownership-checked <see cref="GetOrderQuery"/> handler instead of repeating its rules.
/// </summary>
public sealed class GetOrderSummaryHandler(
    IRequestHandler<GetOrderQuery, Result<OrderDto>> getOrder,
    IBookRepository books) : IRequestHandler<GetOrderSummaryQuery, Result<OrderSummary>>
{
    public async Task<Result<OrderSummary>> HandleAsync(GetOrderSummaryQuery request, CancellationToken cancellationToken)
        => await getOrder.HandleAsync(new GetOrderQuery(request.OrderId), cancellationToken)
            .MatchAsync(
                async order => Result.Success(await SummarizeAsync(order, cancellationToken)),
                errors => Task.FromResult(Result.Failure<OrderSummary>(errors)));

    private async Task<OrderSummary> SummarizeAsync(OrderDto order, CancellationToken cancellationToken)
    {
        var canReorder = true;

        foreach (var line in order.Lines)
        {
            // A book that disappeared from the catalog or ran out of stock cannot be reordered.
            var inStock = (await books.GetAsync(line.Isbn, cancellationToken))
                .Map(book => !book.IsRetired && book.Stock >= line.Quantity)
                .ValueOrDefault(false);

            canReorder &= inStock;
        }

        var items = string.Join(", ", order.Lines.Select(line => $"{line.Quantity} x {line.Title}"));
        var text = $"{order.Status} order of {items} for {order.Total:0.00}.";

        return new OrderSummary(order.Id, text, canReorder);
    }
}
