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
    private sealed record StockReservation(
        IReadOnlyList<StockReservationRequest> Requests,
        IReadOnlyList<OrderLine> Lines)
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

    private async Task<Result<OrderDto>> PlaceAsync(string customerId, PlaceOrderCommand request, CancellationToken cancellationToken)
    {
        var reservation = await ReserveStockAsync(request, cancellationToken);

        if (reservation.IsFailure)
        {
            return reservation.Errors!;
        }

        var reservedStock = reservation.ValueOrThrow();
        var paid = await ChargeAsync(customerId, reservedStock, cancellationToken);

        if (paid.IsFailure)
        {
            await books.ReleaseAsync(reservedStock.Requests, cancellationToken);
            return paid.Errors!;
        }

        var persisted = await PersistAsync(customerId, paid.ValueOrThrow(), cancellationToken);

        if (persisted.IsFailure)
        {
            await books.ReleaseAsync(reservedStock.Requests, cancellationToken);
            return persisted.Errors!;
        }

        return persisted.Map(OrderDto.From);
    }

    // Every missing book and every shortage is reported, not just the first one.
    private async Task<Result<StockReservation>> ReserveStockAsync(PlaceOrderCommand request, CancellationToken cancellationToken)
    {
        var stockRequests = request.Lines
            .Select(line => new StockReservationRequest(line.Isbn, line.Quantity))
            .ToList();

        return (await books.TryReserveAsync(stockRequests, cancellationToken))
            .Bind(reservedBooks => CreateReservation(stockRequests, request.Lines, reservedBooks));
    }

    private static Result<StockReservation> CreateReservation(
        IReadOnlyList<StockReservationRequest> stockRequests,
        IReadOnlyList<OrderItem> requestedLines,
        IReadOnlyList<Book> reservedBooks)
    {
        var quantitiesByIsbn = requestedLines.ToDictionary(line => line.Isbn, line => line.Quantity, StringComparer.OrdinalIgnoreCase);

        var lines = reservedBooks
            .Select(book => new OrderLine(book.Isbn, book.Title, book.Price, quantitiesByIsbn[book.Isbn]))
            .ToList();

        return Result.Success(new StockReservation(stockRequests, lines));
    }

    private async Task<Result<PaidReservation>> ChargeAsync(string customerId, StockReservation reservation, CancellationToken cancellationToken)
    {
        var charge = await ChargeSafelyAsync(new PaymentRequest(customerId, reservation.Total), cancellationToken);

        if (charge.IsFailure)
        {
            logger.LogInformation("Payment for {CustomerId} failed: {Reason}", customerId, charge.FirstErrorMessage);
            return charge.Errors!;
        }

        return charge
            .Tap(_ => logger.LogInformation("Payment for {CustomerId} was accepted.", customerId))
            .Bind(receipt => Result.Success(new PaidReservation(reservation, receipt)));
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
            .TapAsync(order => orders.SaveAsync(order, cancellationToken));
}
