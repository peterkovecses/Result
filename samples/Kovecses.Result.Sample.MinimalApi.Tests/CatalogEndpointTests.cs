namespace Kovecses.Result.Sample.MinimalApi.Tests;

public sealed class CatalogEndpointTests(BookStoreApplicationFactory factory)
    : IClassFixture<BookStoreApplicationFactory>
{
    private readonly HttpClient _client = factory.CreateClient();

    [Fact]
    public async Task GetBook_WhenBookExists_ShouldReturnBook()
    {
        // Arrange
        const string requestUri = "/api/catalog/books/9780132350884";

        // Act
        using var response = await _client.GetAsync(requestUri);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var book = await response.Content.ReadFromJsonAsync<BookDto>();
        Assert.NotNull(book);
        Assert.Equal("Clean Code", book.Title);
    }

    [Fact]
    public async Task GetBook_WhenBookIsMissing_ShouldMapNotFoundError()
    {
        // Arrange
        const string requestUri = "/api/catalog/books/9780000000000";

        // Act
        using var response = await _client.GetAsync(requestUri);

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task SearchBooks_WhenBooksMatch_ShouldReturnPagedBooks()
    {
        // Arrange
        const string requestUri = "/api/catalog/books?term=Clean&page=1&pageSize=2";

        // Act
        using var response = await _client.GetAsync(requestUri);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var page = await response.Content.ReadFromJsonAsync<PagedResult<BookDto>>();
        Assert.NotNull(page);
        Assert.Equal(2, page.Items.Count);
    }

    [Fact]
    public async Task SearchBooks_WhenPageIsInvalid_ShouldReturnValidationProblemDetails()
    {
        // Arrange
        const string requestUri = "/api/catalog/books?term=Clean&page=0&pageSize=2";

        // Act
        using var response = await _client.GetAsync(requestUri);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);
        using var problem = await JsonDocument.ParseAsync(await response.Content.ReadAsStreamAsync());
        Assert.True(problem.RootElement.GetProperty("errors").TryGetProperty("Page", out _));
    }

    [Fact]
    public async Task AddBook_WhenBookIsValid_ShouldReturnCreatedLocation()
    {
        // Arrange
        var command = BookStoreTestData.NewBook();

        // Act
        using var response = await _client.PostAsJsonAsync("/api/catalog/books", command);

        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        Assert.Equal($"/api/catalog/books/{command.Isbn}", response.Headers.Location?.OriginalString);
        var book = await response.Content.ReadFromJsonAsync<BookDto>();
        Assert.NotNull(book);
        Assert.Equal(command.Isbn, book.Isbn);
    }

    [Fact]
    public async Task AddBook_WhenIsbnAlreadyExists_ShouldMapConflictError()
    {
        // Arrange
        var command = BookStoreTestData.NewBook("9780132350884");

        // Act
        using var response = await _client.PostAsJsonAsync("/api/catalog/books", command);

        // Assert
        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }

    [Fact]
    public async Task ChangeBookPrice_WhenPriceIsValid_ShouldReturnUpdatedBook()
    {
        // Arrange
        var isbn = await BookStoreTestData.AddBookAsync(_client, BookStoreTestData.NewBook());

        // Act
        using var response = await _client.PutAsJsonAsync(
            $"/api/catalog/books/{isbn}/price",
            new ChangeBookPriceRequest(32.50m));

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var book = await response.Content.ReadFromJsonAsync<BookDto>();
        Assert.NotNull(book);
        Assert.Equal(32.50m, book.Price);
    }

    [Fact]
    public async Task ChangeBookPrice_WhenPriceIsNotPositive_ShouldReturnValidationProblem()
    {
        // Arrange
        var isbn = await BookStoreTestData.AddBookAsync(_client, BookStoreTestData.NewBook());

        // Act
        using var response = await _client.PutAsJsonAsync(
            $"/api/catalog/books/{isbn}/price",
            new ChangeBookPriceRequest(0));

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task RestockBooks_WhenItemsAreValid_ShouldReturnNoContent()
    {
        // Arrange
        var isbn = await BookStoreTestData.AddBookAsync(_client, BookStoreTestData.NewBook(stock: 1));

        // Act
        using var response = await _client.PostAsJsonAsync(
            "/api/catalog/books/restock",
            new RestockBooksCommand([new RestockItem(isbn, 4)]));

        // Assert
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }

    [Fact]
    public async Task RestockBooks_WhenQuantityIsInvalid_ShouldReturnValidationProblem()
    {
        // Arrange
        var command = new RestockBooksCommand([new RestockItem("9780132350884", 0)]);

        // Act
        using var response = await _client.PostAsJsonAsync(
            "/api/catalog/books/restock",
            command);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task RetireBook_WhenBookHasNoOpenOrders_ShouldReturnNoContent()
    {
        // Arrange
        var isbn = await BookStoreTestData.AddBookAsync(_client, BookStoreTestData.NewBook());

        // Act
        using var response = await _client.DeleteAsync($"/api/catalog/books/{isbn}");

        // Assert
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }

    [Fact]
    public async Task RetireBook_WhenBookHasOpenOrders_ShouldMapFailureError()
    {
        // Arrange
        var isbn = await BookStoreTestData.AddBookAsync(_client, BookStoreTestData.NewBook());
        await BookStoreTestData.PlaceOrderAsync(_client, "retire-customer", isbn);

        // Act
        using var response = await _client.DeleteAsync($"/api/catalog/books/{isbn}");

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task GetBookInternal_WhenBookExists_ShouldReturnWrappedResult()
    {
        // Arrange
        const string requestUri = "/internal/books/9780132350884";

        // Act
        using var response = await _client.GetAsync(requestUri);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<Result<BookDto>>();
        Assert.NotNull(result);
        result.Should().BeSuccess();
        Assert.Equal("Clean Code", result.ValueOrThrow().Title);
    }

    [Fact]
    public async Task GetBookInternal_WhenBookIsMissing_ShouldReturnWrappedFailure()
    {
        // Arrange
        const string requestUri = "/internal/books/9780000000000";

        // Act
        using var response = await _client.GetAsync(requestUri);

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<Result<BookDto>>();
        Assert.NotNull(result);
        result.Should().BeFailure().HaveError(BookErrors.Codes.NotFound).HaveType(ErrorType.NotFound);
    }
}
