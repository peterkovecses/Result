namespace Kovecses.Result.Sample.BookStore.Core.Tests.Infrastructure;

public class InMemoryBookRepositoryTests
{
    private readonly InMemoryBookRepository _sut = new([TestData.CleanCode(stock: 8)]);

    [Fact]
    public async Task ReleaseAsync_BookRetiredAfterReservation_ShouldStillReturnReservedStock()
    {
        // Arrange
        var requests = new[] { new StockReservationRequest(TestData.CleanCodeIsbn, 3) };
        await _sut.TryReserveAsync(requests, default);
        var reserved = (await _sut.GetAsync(TestData.CleanCodeIsbn, default)).ValueOrThrow();
        await _sut.SaveAsync(reserved.Retire().ValueOrThrow(), default);

        // Act
        await _sut.ReleaseAsync(requests, default);

        // Assert
        (await _sut.GetAsync(TestData.CleanCodeIsbn, default)).Should().BeSuccess()
            .HaveData(book => book!.Stock == 8 && book.IsRetired);
    }
}
