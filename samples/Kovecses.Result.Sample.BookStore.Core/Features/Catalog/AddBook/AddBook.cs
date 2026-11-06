namespace Kovecses.Result.Sample.BookStore.Core.Features.Catalog.AddBook;

public sealed record AddBookCommand(string Isbn, string Title, string Author, decimal Price, int InitialStock)
    : IRequest<Result<BookDto>>;

public sealed class AddBookValidator : IValidator<AddBookCommand>
{
    public Task<Result> ValidateAsync(AddBookCommand request, CancellationToken cancellationToken)
        => new ValidationErrors()
            .AddIf(string.IsNullOrWhiteSpace(request.Isbn), nameof(request.Isbn), "The ISBN is required.")
            .AddIf(!string.IsNullOrWhiteSpace(request.Isbn) && !IsIsbn13(request.Isbn), nameof(request.Isbn), "The ISBN must be 13 digits.")
            .AddIf(string.IsNullOrWhiteSpace(request.Title), nameof(request.Title), "The title is required.")
            .AddIf(request.Title?.Length > 200, nameof(request.Title), "The title cannot be longer than 200 characters.")
            .AddIf(string.IsNullOrWhiteSpace(request.Author), nameof(request.Author), "The author is required.")
            .AddIf(request.Price <= 0, nameof(request.Price), "The price must be greater than zero.")
            .AddIf(request.InitialStock < 0, nameof(request.InitialStock), "The initial stock cannot be negative.")
            .ToResultAsync();

    private static bool IsIsbn13(string isbn) => isbn.Length == 13 && isbn.All(char.IsAsciiDigit);
}

public sealed class AddBookHandler(IBookRepository books) : IRequestHandler<AddBookCommand, Result<BookDto>>
{
    public async Task<Result<BookDto>> HandleAsync(AddBookCommand request, CancellationToken cancellationToken)
    {
        if (await books.ExistsAsync(request.Isbn, cancellationToken))
        {
            return BookErrors.DuplicateIsbn(request.Isbn);
        }

        return await Book.Create(request.Isbn, request.Title, request.Author, request.Price, request.InitialStock)
            .TapAsync(book => books.SaveAsync(book, cancellationToken))
            .MapAsync(book => BookDto.From(book));
    }
}
