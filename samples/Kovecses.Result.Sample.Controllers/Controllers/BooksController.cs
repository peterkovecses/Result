namespace Kovecses.Result.Sample.Controllers.Controllers;

[ApiController]
[Route("books")]
public sealed class BooksController(IMediator mediator) : ApiControllerBase(mediator)
{
    [HttpGet("{isbn}")]
    public Task<IActionResult> Get(string isbn, CancellationToken cancellationToken)
        => Mediator.SendAsync<GetBookQuery, Result<BookDto>>(new GetBookQuery(isbn), cancellationToken)
            .ToActionResultAsync();

    [HttpGet]
    public Task<IActionResult> Search(
        [FromQuery] string? term,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken cancellationToken = default)
        => Mediator.SendAsync<SearchBooksQuery, Result<PagedResult<BookDto>>>(
                new SearchBooksQuery(term, page, pageSize),
                cancellationToken)
            .ToActionResultAsync();

    [HttpPost]
    public Task<IActionResult> Add(AddBookCommand command, CancellationToken cancellationToken)
        => Mediator.SendAsync<AddBookCommand, Result<BookDto>>(command, cancellationToken)
            .MatchToActionResultAsync(
                book => CreatedAtAction(nameof(Get), new { isbn = book.Isbn }, book));

    [HttpPut("{isbn}/price")]
    public Task<IActionResult> ChangePrice(
        string isbn,
        ChangeBookPriceRequest request,
        CancellationToken cancellationToken)
        => Mediator.SendAsync<ChangeBookPriceCommand, Result<BookDto>>(
                new ChangeBookPriceCommand(isbn, request.NewPrice),
                cancellationToken)
            .ToActionResultAsync();

    [HttpPost("restock")]
    public async Task<IActionResult> Restock(RestockBooksCommand command, CancellationToken cancellationToken)
    {
        var result = await Mediator.SendAsync<RestockBooksCommand, Result>(command, cancellationToken);

        if (result.IsSuccess
            && result.Metadata?.TryGetValue("restockedBooks", out var count) == true
            && count is not null)
        {
            Response.Headers["X-Restocked-Books"] = count.ToString();
        }

        return result.ToActionResult();
    }

    [HttpDelete("{isbn}")]
    public Task<IActionResult> Retire(string isbn, CancellationToken cancellationToken)
        => Mediator.SendAsync<RetireBookCommand, Result>(new RetireBookCommand(isbn), cancellationToken)
            .ToActionResultAsync();
}

public sealed record ChangeBookPriceRequest(decimal NewPrice);
