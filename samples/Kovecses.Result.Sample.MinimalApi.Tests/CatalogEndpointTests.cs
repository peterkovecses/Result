using System.Net;
using System.Text.Json;
using Kovecses.Result.Sample.MinimalApi.Tests.Support;

namespace Kovecses.Result.Sample.MinimalApi.Tests;

public sealed class CatalogEndpointTests(BookStoreApplicationFactory factory)
    : IClassFixture<BookStoreApplicationFactory>
{
    private readonly HttpClient _client = factory.CreateClient();

    [Fact]
    public async Task GetBook_ExistingBook_ReturnsBook()
    {
        using var response = await _client.GetAsync("/api/catalog/books/9780132350884");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var book = await response.Content.ReadFromJsonAsync<BookDto>();
        Assert.NotNull(book);
        Assert.Equal("Clean Code", book.Title);
    }

    [Fact]
    public async Task GetBook_MissingBook_MapsNotFoundError()
    {
        using var response = await _client.GetAsync("/api/catalog/books/9780000000000");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task SearchBooks_MatchingBooks_ReturnsPagedBooks()
    {
        using var response = await _client.GetAsync("/api/catalog/books?term=Clean&page=1&pageSize=2");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var page = await response.Content.ReadFromJsonAsync<PagedResult<BookDto>>();
        Assert.NotNull(page);
        Assert.Equal(2, page.Items.Count);
    }

    [Fact]
    public async Task SearchBooks_InvalidPage_ReturnsValidationProblemDetails()
    {
        using var response = await _client.GetAsync("/api/catalog/books?term=Clean&page=0&pageSize=2");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);
        using var problem = await JsonDocument.ParseAsync(await response.Content.ReadAsStreamAsync());
        Assert.True(problem.RootElement.GetProperty("errors").TryGetProperty("Page", out _));
    }

    [Fact]
    public async Task AddBook_ValidBook_ReturnsCreatedLocation()
    {
        var command = BookStoreTestData.NewBook();
        using var response = await _client.PostAsJsonAsync("/api/catalog/books", command);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        Assert.Equal($"/api/catalog/books/{command.Isbn}", response.Headers.Location?.OriginalString);
        var book = await response.Content.ReadFromJsonAsync<BookDto>();
        Assert.NotNull(book);
        Assert.Equal(command.Isbn, book.Isbn);
    }

    [Fact]
    public async Task AddBook_DuplicateIsbn_MapsConflictError()
    {
        var command = BookStoreTestData.NewBook("9780132350884");
        using var response = await _client.PostAsJsonAsync("/api/catalog/books", command);

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }

    [Fact]
    public async Task ChangeBookPrice_ValidPrice_ReturnsUpdatedBook()
    {
        var isbn = await BookStoreTestData.AddBookAsync(_client, BookStoreTestData.NewBook());
        using var response = await _client.PutAsJsonAsync(
            $"/api/catalog/books/{isbn}/price",
            new ChangeBookPriceRequest(32.50m));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var book = await response.Content.ReadFromJsonAsync<BookDto>();
        Assert.NotNull(book);
        Assert.Equal(32.50m, book.Price);
    }

    [Fact]
    public async Task ChangeBookPrice_NonPositivePrice_ReturnsValidationProblem()
    {
        var isbn = await BookStoreTestData.AddBookAsync(_client, BookStoreTestData.NewBook());
        using var response = await _client.PutAsJsonAsync(
            $"/api/catalog/books/{isbn}/price",
            new ChangeBookPriceRequest(0));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task RestockBooks_ValidItems_ReturnsNoContent()
    {
        var isbn = await BookStoreTestData.AddBookAsync(_client, BookStoreTestData.NewBook(stock: 1));
        using var response = await _client.PostAsJsonAsync(
            "/api/catalog/books/restock",
            new RestockBooksCommand([new RestockItem(isbn, 4)]));

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }

    [Fact]
    public async Task RestockBooks_InvalidQuantity_ReturnsValidationProblem()
    {
        using var response = await _client.PostAsJsonAsync(
            "/api/catalog/books/restock",
            new RestockBooksCommand([new RestockItem("9780132350884", 0)]));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task RetireBook_WithoutOpenOrders_ReturnsNoContent()
    {
        var isbn = await BookStoreTestData.AddBookAsync(_client, BookStoreTestData.NewBook());
        using var response = await _client.DeleteAsync($"/api/catalog/books/{isbn}");

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }

    [Fact]
    public async Task RetireBook_WithOpenOrder_MapsFailureError()
    {
        var isbn = await BookStoreTestData.AddBookAsync(_client, BookStoreTestData.NewBook());
        await BookStoreTestData.PlaceOrderAsync(_client, "retire-customer", isbn);
        using var response = await _client.DeleteAsync($"/api/catalog/books/{isbn}");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task GetBookInternal_ExistingBook_ReturnsWrappedResult()
    {
        using var response = await _client.GetAsync("/internal/books/9780132350884");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<Result<BookDto>>();
        Assert.NotNull(result);
        result.Should().BeSuccess();
        Assert.Equal("Clean Code", result.ValueOrThrow().Title);
    }

    [Fact]
    public async Task GetBookInternal_MissingBook_ReturnsWrappedFailure()
    {
        using var response = await _client.GetAsync("/internal/books/9780000000000");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<Result<BookDto>>();
        Assert.NotNull(result);
        result.Should().BeFailure().HaveError(BookErrors.Codes.NotFound).HaveType(ErrorType.NotFound);
    }
}
