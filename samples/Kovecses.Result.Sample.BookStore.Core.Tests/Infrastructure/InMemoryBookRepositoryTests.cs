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

    [Fact]
    public async Task UpdateAsync_ConcurrentWithReservation_ShouldNotOverwriteReservedStock()
    {
        // Arrange
        var requests = new[] { new StockReservationRequest(TestData.CleanCodeIsbn, 1) };

        // Act
        var updates = Enumerable.Range(0, 8).Select(_ => Task.Run(() => _sut.UpdateAsync(
            TestData.CleanCodeIsbn, book => book.ChangePrice(book.Price + 1), default)));
        var reservations = Enumerable.Range(0, 8).Select(_ => Task.Run(() => _sut.TryReserveAsync(requests, default)));
        await Task.WhenAll(updates.Concat(reservations.Cast<Task>()));

        // Assert
        (await _sut.GetAsync(TestData.CleanCodeIsbn, default)).Should().BeSuccess()
            .HaveData(book => book!.Stock == 0 && book.Price == TestData.CleanCode().Price + 8);
    }
}
