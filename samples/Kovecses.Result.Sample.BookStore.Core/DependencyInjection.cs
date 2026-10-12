using Kovecses.Result.Sample.BookStore.Core.Features.Catalog.AddBook;
using Kovecses.Result.Sample.BookStore.Core.Features.Catalog.ChangeBookPrice;
using Kovecses.Result.Sample.BookStore.Core.Features.Catalog.GetBook;
using Kovecses.Result.Sample.BookStore.Core.Features.Catalog.RestockBooks;
using Kovecses.Result.Sample.BookStore.Core.Features.Catalog.RetireBook;
using Kovecses.Result.Sample.BookStore.Core.Features.Catalog.SearchBooks;
using Kovecses.Result.Sample.BookStore.Core.Features.Catalog;
using Kovecses.Result.Sample.BookStore.Core.Features.Orders;
using Kovecses.Result.Sample.BookStore.Core.Features.Orders.CancelOrder;
using Kovecses.Result.Sample.BookStore.Core.Features.Orders.GetOrder;
using Kovecses.Result.Sample.BookStore.Core.Features.Orders.GetOrderSummary;
using Kovecses.Result.Sample.BookStore.Core.Features.Orders.PlaceOrder;
using Kovecses.Result.Sample.BookStore.Core.Infrastructure;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Kovecses.Result.Sample.BookStore.Core;

public static class DependencyInjection
{
    /// <summary>
    /// Registers the bookstore core. The host must provide an <see cref="ICurrentUser"/> implementation and logging.
    /// </summary>
    public static IServiceCollection AddBookStoreCore(this IServiceCollection services)
    {
        services.TryAddSingleton(TimeProvider.System);

        services.AddSingleton<IBookRepository>(_ => new InMemoryBookRepository(BookSeedData.Create()));
        services.AddSingleton<IOrderRepository, InMemoryOrderRepository>();
        services.AddSingleton<IPaymentGateway, FakePaymentGateway>();

        services.AddScoped<IMediator, Mediator>();

        // Catalog
        services.AddHandler<GetBookQuery, Result<BookDto>, GetBookHandler>();
        services.AddHandler<SearchBooksQuery, Result<PagedResult<BookDto>>, SearchBooksHandler>();
        services.AddHandler<AddBookCommand, Result<BookDto>, AddBookHandler>();
        services.AddHandler<ChangeBookPriceCommand, Result<BookDto>, ChangeBookPriceHandler>();
        services.AddHandler<RestockBooksCommand, Result, RestockBooksHandler>();
        services.AddHandler<RetireBookCommand, Result, RetireBookHandler>();

        // Orders
        services.AddHandler<PlaceOrderCommand, Result<OrderDto>, PlaceOrderHandler>();
        services.AddHandler<GetOrderQuery, Result<OrderDto>, GetOrderHandler>();
        services.AddHandler<CancelOrderCommand, Result, CancelOrderHandler>();
        services.AddHandler<GetOrderSummaryQuery, Result<OrderSummary>, GetOrderSummaryHandler>();

        services.AddTransient<IValidator<SearchBooksQuery>, SearchBooksValidator>();
        services.AddTransient<IValidator<AddBookCommand>, AddBookValidator>();
        services.AddTransient<IValidator<ChangeBookPriceCommand>, ChangeBookPriceValidator>();
        services.AddTransient<IValidator<RestockBooksCommand>, RestockBooksValidator>();
        services.AddTransient<IValidator<PlaceOrderCommand>, PlaceOrderValidator>();

        return services;
    }

    // Handlers are exposed through the validation decorator; requests without validators pass straight through.
    private static IServiceCollection AddHandler<TRequest, TResult, THandler>(this IServiceCollection services)
        where TRequest : IRequest<TResult>
        where TResult : Result
        where THandler : class, IRequestHandler<TRequest, TResult>
    {
        services.AddTransient<THandler>();
        services.AddTransient<IRequestHandler<TRequest, TResult>>(provider => new ValidationDecorator<TRequest, TResult>(
            provider.GetRequiredService<THandler>(),
            provider.GetServices<IValidator<TRequest>>()));

        return services;
    }
}
