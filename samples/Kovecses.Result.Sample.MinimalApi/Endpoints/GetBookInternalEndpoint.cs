namespace Kovecses.Result.Sample.MinimalApi.Endpoints;

public static class GetBookInternalEndpoint
{
    public static RouteHandlerBuilder MapEndpoint(RouteGroupBuilder group)
        => group.MapGet("/{isbn}", async (
                string isbn,
                IRequestHandler<GetBookQuery, Result<BookDto>> handler,
                CancellationToken cancellationToken) =>
                await handler.HandleAsync(new GetBookQuery(isbn), cancellationToken)
                    .ToMinimalApiResultAsync(includeResultInResponse: true))
            .WithName("GetBookInternal")
            .WithDescription("Returns the complete Result envelope for an internal typed client.")
            .Produces<Result<BookDto>>(StatusCodes.Status200OK)
            .Produces<Result<BookDto>>(StatusCodes.Status404NotFound);
}
