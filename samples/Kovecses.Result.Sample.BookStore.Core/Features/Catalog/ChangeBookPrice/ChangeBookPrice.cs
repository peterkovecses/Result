namespace Kovecses.Result.Sample.BookStore.Core.Features.Catalog.ChangeBookPrice;

public sealed record ChangeBookPriceCommand(string Isbn, decimal NewPrice) : IRequest<Result<BookDto>>;

public sealed class ChangeBookPriceValidator : IValidator<ChangeBookPriceCommand>
{
    public Task<Result> ValidateAsync(ChangeBookPriceCommand request, CancellationToken cancellationToken)
        => new ValidationErrors()
            .AddIf(request.NewPrice <= 0, nameof(request.NewPrice), "The price must be greater than zero.")
            .ToResultAsync();
}

public sealed class ChangeBookPriceHandler(IBookRepository books) : IRequestHandler<ChangeBookPriceCommand, Result<BookDto>>
{
    public async Task<Result<BookDto>> HandleAsync(ChangeBookPriceCommand request, CancellationToken cancellationToken)
        => await books.GetAsync(request.Isbn, cancellationToken)
            .BindAsync(book => book.ChangePrice(request.NewPrice))
            .TapAsync(book => books.SaveAsync(book, cancellationToken))
            .MapAsync(book => BookDto.From(book));
}
