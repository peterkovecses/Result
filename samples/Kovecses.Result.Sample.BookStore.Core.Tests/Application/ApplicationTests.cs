
namespace Kovecses.Result.Sample.BookStore.Core.Tests.Application;

public class MediatorTests
{
    private readonly ServiceProvider _provider;
    private readonly IMediator _sut;

    public MediatorTests()
    {
        var services = new ServiceCollection();
        services.AddSingleton(typeof(ILogger<>), typeof(NullLogger<>));
        services.AddSingleton<ICurrentUser>(new TestCurrentUser());
        services.AddBookStoreCore();

        _provider = services.BuildServiceProvider();
        _sut = _provider.CreateScope().ServiceProvider.GetRequiredService<IMediator>();
    }

    [Fact]
    public async Task SendAsync_RequestWithoutValidator_ShouldReachHandler()
    {
        // Arrange
        var query = new GetBookQuery("9780132350884");

        // Act
        var result = await _sut.SendAsync<GetBookQuery, Result<BookDto>>(query);

        // Assert
        result.Should().BeSuccess()
            .HaveData(book => book!.Title == "Clean Code");
    }

    [Fact]
    public async Task SendAsync_InvalidGenericResultRequest_ShouldReturnValidationFailure()
    {
        // Arrange
        var command = new AddBookCommand("", "", "", 0m, 0);

        // Act
        var result = await _sut.SendAsync<AddBookCommand, Result<BookDto>>(command);

        // Assert
        result.Should().BeFailure()
            .HaveError(ErrorCodes.Validation)
            .HaveValidationProperty("Isbn").Contain("required");
        result.Should().ThrowOnValueAccess<InvalidOperationException>();
    }

    [Fact]
    public async Task SendAsync_InvalidNonGenericResultRequest_ShouldReturnValidationFailure()
    {
        // Arrange
        var command = new RestockBooksCommand([]);

        // Act
        var result = await _sut.SendAsync<RestockBooksCommand, Result>(command);

        // Assert
        result.Should().BeFailure()
            .HaveError(ErrorCodes.Validation)
            .HaveType(ErrorType.Validation);
    }

    [Fact]
    public async Task HandleAsync_HandlerInjectedDirectly_ShouldStillBeValidated()
    {
        // Arrange
        var handler = _provider.GetRequiredService<IRequestHandler<AddBookCommand, Result<BookDto>>>();
        var command = new AddBookCommand("123", "Title", "Author", 10m, 1);

        // Act
        var result = await handler.HandleAsync(command, default);

        // Assert
        result.Should().BeFailure()
            .HaveError(ErrorCodes.Validation)
            .HaveValidationProperty("Isbn").Contain("13 digits");
    }

    [Fact]
    public async Task SendAsync_ValidRequest_ShouldReachHandler()
    {
        // Arrange
        var command = new AddBookCommand("9780201633610", "Design Patterns", "Erich Gamma", 50m, 4);

        // Act
        var result = await _sut.SendAsync<AddBookCommand, Result<BookDto>>(command);

        // Assert
        result.Should().BeSuccess()
            .HaveData(book => book!.Isbn == "9780201633610");
    }
}

public class CurrentUserExtensionsTests
{
    [Fact]
    public void RequireUserId_SignedInUser_ShouldReturnUserId()
    {
        // Arrange
        var _sut = new TestCurrentUser("alice");

        // Act
        var result = _sut.RequireUserId();

        // Assert
        result.Should().BeSuccess()
            .HaveData("alice");
    }

    [Fact]
    public void RequireUserId_AnonymousUser_ShouldReturnUnauthorized()
    {
        // Arrange
        var _sut = new TestCurrentUser(null);

        // Act
        var result = _sut.RequireUserId();

        // Assert
        result.Should().BeFailure()
            .HaveError(ErrorCodes.Unauthorized);
    }
}

public class FakePaymentGatewayTests
{
    private readonly FakePaymentGateway _sut = new();

    [Fact]
    public async Task ChargeAsync_AmountWithinLimit_ShouldReturnReceipt()
    {
        // Arrange
        var request = new PaymentRequest("alice", 100m);

        // Act
        var result = await _sut.ChargeAsync(request, default);

        // Assert
        result.Should().BeSuccess()
            .HaveData(receipt => receipt!.Reference.StartsWith("pay_"));
    }

    [Fact]
    public async Task ChargeAsync_AmountOverLimit_ShouldBeDeclined()
    {
        // Arrange
        var request = new PaymentRequest("alice", FakePaymentGateway.PerOrderLimit + 1);

        // Act
        var result = await _sut.ChargeAsync(request, default);

        // Assert
        result.Should().BeFailure()
            .HaveError(OrderErrors.Codes.PaymentDeclined);
    }
}
