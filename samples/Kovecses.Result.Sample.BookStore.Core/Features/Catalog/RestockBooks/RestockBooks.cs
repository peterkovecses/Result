namespace Kovecses.Result.Sample.BookStore.Core.Features.Catalog.RestockBooks;

public sealed record RestockItem(string Isbn, int Quantity);

public sealed record RestockBooksCommand(IReadOnlyList<RestockItem> Items) : IRequest<Result>;

public sealed class RestockBooksValidator : IValidator<RestockBooksCommand>
{
    public Task<Result> ValidateAsync(RestockBooksCommand request, CancellationToken cancellationToken)
    {
        var items = request.Items;
        var validItems = items?.OfType<RestockItem>().ToArray() ?? [];

        return new ValidationErrors()
            .AddIf(items is null || items.Count == 0, nameof(request.Items), "At least one item is required.")
            .AddIf(items is not null && validItems.Length != items.Count, nameof(request.Items), "Every item is required.")
            .AddIf(validItems.Any(item => string.IsNullOrWhiteSpace(item.Isbn)), nameof(request.Items), "Every item needs an ISBN.")
            .AddIf(validItems.Any(item => item.Quantity < 1), nameof(request.Items), "Every quantity must be 1 or greater.")
            .AddIf(validItems.GroupBy(item => item.Isbn).Any(group => group.Count() > 1), nameof(request.Items), "Each ISBN can only appear once.")
            .ToResultAsync();
    }
}

/// <summary>
/// Restocks several books at once. All-or-nothing: if any item is rejected, nothing is saved
/// and every rejection is reported.
/// </summary>
public sealed class RestockBooksHandler(IBookRepository books, ILogger<RestockBooksHandler> logger)
    : IRequestHandler<RestockBooksCommand, Result>
{
    public async Task<Result> HandleAsync(RestockBooksCommand request, CancellationToken cancellationToken)
    {
        var loaded = await books.GetManyAsync(request.Items.Select(item => item.Isbn), cancellationToken);

        if (loaded.IsFailure)
        {
            return loaded.ToResult();
        }

        var restocked = loaded.ValueOrThrow()
            .Zip(request.Items, (book, item) => book.Restock(item.Quantity))
            .ToArray();

        var rejections = Result.Combine(restocked);

        if (rejections.IsFailure)
        {
            logger.LogWarning("Restock rejected: {Reasons}", rejections.JoinErrorMessages());
        }

        return await rejections.BindAsync(() => SaveAllAsync(request.Items, cancellationToken));
    }

    private async Task<Result> SaveAllAsync(IReadOnlyList<RestockItem> items, CancellationToken cancellationToken)
    {
        var applied = new List<Result<Book>>(items.Count);

        foreach (var item in items)
        {
            applied.Add(await books.UpdateAsync(item.Isbn, book => book.Restock(item.Quantity), cancellationToken));
        }

        return Result.Combine(applied.ToArray())
            .Bind(() => Result.Success(new Dictionary<string, object> { ["restockedBooks"] = items.Count }));
    }
}
