namespace Kovecses.Result.Sample.MinimalApi.Endpoints;

public sealed record ChangeBookPriceRequest(decimal NewPrice);

public static class ChangeBookPriceEndpoint
{
    public static RouteHandlerBuilder MapEndpoint(RouteGroupBuilder group)
        => group.MapPut("/books/{isbn}/price", async (
                string isbn,
                ChangeBookPriceRequest request,
                IRequestHandler<ChangeBookPriceCommand, Result<BookDto>> handler,
                CancellationToken cancellationToken) =>
                await handler.HandleAsync(new ChangeBookPriceCommand(isbn, request.NewPrice), cancellationToken)
                    .ToMinimalApiResultAsync())
            .WithName("ChangeBookPrice")
            .WithDescription("Changes a catalog book's price.")
            .Produces<BookDto>(StatusCodes.Status200OK)
            .ProducesValidationProblem()
            .ProducesProblem(StatusCodes.Status404NotFound);
}
