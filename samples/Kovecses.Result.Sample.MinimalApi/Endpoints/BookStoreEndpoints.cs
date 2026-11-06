namespace Kovecses.Result.Sample.MinimalApi.Endpoints;

public static class BookStoreEndpoints
{
    public static IEndpointRouteBuilder MapBookStoreEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var catalog = endpoints.MapGroup("/api/catalog").WithTags("Catalog");
        GetBookEndpoint.MapEndpoint(catalog);
        SearchBooksEndpoint.MapEndpoint(catalog);
        AddBookEndpoint.MapEndpoint(catalog);
        ChangeBookPriceEndpoint.MapEndpoint(catalog);
        RestockBooksEndpoint.MapEndpoint(catalog);
        RetireBookEndpoint.MapEndpoint(catalog);

        var orders = endpoints.MapGroup("/api/orders").WithTags("Orders");
        PlaceOrderEndpoint.MapEndpoint(orders);
        GetOrderEndpoint.MapEndpoint(orders);
        CancelOrderEndpoint.MapEndpoint(orders);
        GetOrderSummaryEndpoint.MapEndpoint(orders);

        GetBookInternalEndpoint.MapEndpoint(endpoints.MapGroup("/internal/books").WithTags("Internal Book Store"));

        return endpoints;
    }
}
