namespace Kovecses.Result.Sample.BookStore.Core.Features.Catalog.RetireBook;

public sealed record RetireBookCommand(string Isbn) : IRequest<Result>;

public sealed class RetireBookHandler(IBookRepository books, IOrderRepository orders) : IRequestHandler<RetireBookCommand, Result>
{
    public async Task<Result> HandleAsync(RetireBookCommand request, CancellationToken cancellationToken)
    {
        var retired = await books.GetAsync(request.Isbn, cancellationToken)
            .EnsureAsync(
                async _ => !await orders.HasOpenOrdersForAsync(request.Isbn, cancellationToken),
                BookErrors.HasOpenOrders(request.Isbn))
            .BindAsync(book => books.UpdateAsync(book.Isbn, current => current.Retire(), cancellationToken));

        return retired.ToResult();
    }
}
