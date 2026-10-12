namespace Kovecses.Result.Sample.BookStore.Core.Features.Orders.PlaceOrder;

public sealed record OrderItem(string Isbn, int Quantity);

public sealed record PlaceOrderCommand(IReadOnlyList<OrderItem> Lines) : IRequest<Result<OrderDto>>;

public sealed class PlaceOrderValidator : IValidator<PlaceOrderCommand>
{
    public const int MaxQuantityPerLine = 20;

    public Task<Result> ValidateAsync(PlaceOrderCommand request, CancellationToken cancellationToken)
        => new ValidationErrors()
            .AddIf(request.Lines.Count == 0, nameof(request.Lines), "An order needs at least one line.")
            .AddIf(request.Lines.Any(line => string.IsNullOrWhiteSpace(line.Isbn)), nameof(request.Lines), "Every line needs an ISBN.")
            .AddIf(
                request.Lines.Any(line => line.Quantity is < 1 or > MaxQuantityPerLine),
                nameof(request.Lines),
                $"Every quantity must be between 1 and {MaxQuantityPerLine}.")
            .AddIf(request.Lines.GroupBy(line => line.Isbn).Any(group => group.Count() > 1), nameof(request.Lines), "Each ISBN can only appear once.")
            .ToResultAsync();
}

public sealed class PlaceOrderHandler(
    IBookRepository books,
    IOrderRepository orders,
    IPaymentGateway paymentGateway,
    ICurrentUser currentUser,
    TimeProvider timeProvider,
    ILogger<PlaceOrderHandler> logger) : IRequestHandler<PlaceOrderCommand, Result<OrderDto>>
{
    private sealed record StockReservation(IReadOnlyList<Book> Books, IReadOnlyList<OrderLine> Lines)
    {
        public decimal Total => Lines.Sum(line => line.Total);
    }

    private sealed record PaidReservation(StockReservation Reservation, PaymentReceipt Receipt);

    public async Task<Result<OrderDto>> HandleAsync(PlaceOrderCommand request, CancellationToken cancellationToken)
    {
        var placed = await currentUser.RequireUserId().BindAsync(userId => PlaceAsync(userId, request, cancellationToken));

        if (placed.IsFailure)
        {
            logger.LogWarning("Order was rejected: {Reasons}", placed.JoinErrorMessages());
        }

        return placed;
    }

    private Task<Result<OrderDto>> PlaceAsync(string customerId, PlaceOrderCommand request, CancellationToken cancellationToken)
        => ReserveStockAsync(request, cancellationToken)
            .BindAsync(reservation => ChargeAsync(customerId, reservation, cancellationToken))
            .BindAsync(paid => PersistAsync(customerId, paid, cancellationToken))
            .MapAsync(order => OrderDto.From(order));

    // Every missing book and every shortage is reported, not just the first one.
    private async Task<Result<StockReservation>> ReserveStockAsync(PlaceOrderCommand request, CancellationToken cancellationToken)
    {
        var loaded = await books.GetManyAsync(request.Lines.Select(line => line.Isbn), cancellationToken);

        if (loaded.IsFailure)
        {
            return loaded.Errors!;
        }

        var catalog = loaded.ValueOrThrow();
        var reservations = catalog.Zip(request.Lines, (book, line) => book.Reserve(line.Quantity)).ToArray();

        return Result.Combine(reservations).Match(
            () => Result.Success(new StockReservation(
                reservations.Select(reservation => reservation.ValueOrThrow()).ToList(),
                catalog.Zip(request.Lines, (book, line) => new OrderLine(book.Isbn, book.Title, book.Price, line.Quantity)).ToList())),
            errors => Result.Failure<StockReservation>(errors));
    }

    private async Task<Result<PaidReservation>> ChargeAsync(string customerId, StockReservation reservation, CancellationToken cancellationToken)
    {
        var charge = await ChargeSafelyAsync(new PaymentRequest(customerId, reservation.Total), cancellationToken);

        return charge.MatchFirst(
            receipt => Result.Success(new PaidReservation(reservation, receipt)),
            error =>
            {
                logger.LogInformation("Payment for {CustomerId} failed: {Reason}", customerId, charge.FirstErrorMessage);

                return Result.Failure<PaidReservation>(error);
            });
    }

    // Declines come back as failed results, infrastructure problems are exceptions that we translate here.
    private async Task<Result<PaymentReceipt>> ChargeSafelyAsync(PaymentRequest payment, CancellationToken cancellationToken)
    {
        try
        {
            return await paymentGateway.ChargeAsync(payment, cancellationToken);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            return OrderErrors.PaymentCanceled();
        }
        catch (Exception ex) when (ex is TimeoutException or OperationCanceledException)
        {
            return OrderErrors.PaymentTimedOut();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "The payment gateway failed unexpectedly.");

            return OrderErrors.PaymentGatewayFailed();
        }
    }

    private Task<Result<Order>> PersistAsync(string customerId, PaidReservation paid, CancellationToken cancellationToken)
        => Order.Place(customerId, paid.Reservation.Lines, paid.Receipt.Reference, timeProvider.GetUtcNow())
            .TapAsync(async order =>
            {
                foreach (var book in paid.Reservation.Books)
                {
                    await books.SaveAsync(book, cancellationToken);
                }

                await orders.SaveAsync(order, cancellationToken);
            });
}
