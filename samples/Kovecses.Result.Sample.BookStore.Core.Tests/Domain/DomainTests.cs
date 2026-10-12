namespace Kovecses.Result.Sample.BookStore.Core.Tests.Domain;

public class BookTests
{
    [Fact]
    public void Create_NonPositivePrice_ShouldReturnInvalidPrice()
    {
        // Arrange & Act
        var result = Book.Create(TestData.CleanCodeIsbn, "Clean Code", "Robert C. Martin", 0m, 1);

        // Assert
        result.Should().BeFailure()
            .HaveError(BookErrors.Codes.InvalidPrice);
    }

    [Fact]
    public void Reserve_MoreThanInStock_ShouldReturnConflictWithMetadata()
    {
        // Arrange
        var _sut = TestData.CleanCode(stock: 2);

        // Act
        var result = _sut.Reserve(5);

        // Assert
        result.Should().BeFailure()
            .HaveError(BookErrors.Codes.InsufficientStock)
            .HaveMetadata("requested", 5);
        result.Should().BeFailure()
            .HaveError(BookErrors.Codes.InsufficientStock)
            .HaveMetadata("available", 2);
    }

    [Fact]
    public void Reserve_WithinStock_ShouldDecreaseStock()
    {
        // Arrange
        var _sut = TestData.CleanCode(stock: 5);

        // Act
        var result = _sut.Reserve(2);

        // Assert
        result.Should().BeSuccess()
            .HaveData(book => book!.Stock == 3);
    }

    [Fact]
    public void Restock_NonPositiveQuantity_ShouldReturnFailure()
    {
        // Arrange
        var _sut = TestData.CleanCode();

        // Act
        var result = _sut.Restock(0);

        // Assert
        result.Should().BeFailure()
            .HaveError(BookErrors.Codes.InvalidStock);
    }

    [Fact]
    public void Retire_AlreadyRetiredBook_ShouldReturnConflict()
    {
        // Arrange
        var _sut = TestData.CleanCode() with { IsRetired = true };

        // Act
        var result = _sut.Retire();

        // Assert
        result.Should().BeFailure()
            .HaveError(BookErrors.Codes.Retired);
    }
}

public class OrderTests
{
    [Fact]
    public void Place_WithoutLines_ShouldReturnEmptyOrderFailure()
    {
        // Arrange & Act
        var result = Order.Place("alice", [], "pay_1", DateTimeOffset.UtcNow);

        // Assert
        result.Should().BeFailure()
            .HaveError(OrderErrors.Codes.EmptyOrder)
            .HaveType(ErrorType.Failure);
    }

    [Fact]
    public void Cancel_OpenOrder_ShouldMarkOrderCancelled()
    {
        // Arrange
        var _sut = TestData.OrderFor("alice", TestData.LineOf(TestData.CleanCode(), 1));

        // Act
        var result = _sut.Cancel();

        // Assert
        result.Should().BeSuccess()
            .HaveData(order => order!.Status == OrderStatus.Cancelled);
    }

    [Fact]
    public void Cancel_CancelledOrder_ShouldReturnConflict()
    {
        // Arrange
        var _sut = TestData.OrderFor("alice", TestData.LineOf(TestData.CleanCode(), 1)).Cancel().ValueOrThrow();

        // Act
        var result = _sut.Cancel();

        // Assert
        result.Should().BeFailure()
            .HaveError(OrderErrors.Codes.AlreadyCancelled);
    }
}
