using Kovecses.Result.FluentAssertions;
using Kovecses.Result.Sample.Core;
using Xunit;

namespace Kovecses.Result.Sample.Tests;

/// <summary>
/// Unit tests for EmployeeHandlers to demonstrate Kovecses.Result.FluentAssertions usage.
/// </summary>
public class EmployeeHandlersTests
{
    private readonly EmployeeHandlers _sut = new();

    [Fact]
    public async Task HandleAsync_GetExistingEmployee_ShouldReturnSuccess()
    {
        // Arrange
        var query = new GetEmployeeQuery(1);

        // Act
        var result = await _sut.HandleAsync(query, default);

        // Assert
        result.Should().BeSuccess()
            .HaveData(e => e?.Id == 1 && e.FullName == "The Boss");
    }

    [Fact]
    public async Task HandleAsync_GetNonExistingEmployee_ShouldReturnNotFound()
    {
        // Arrange
        var query = new GetEmployeeQuery(999);

        // Act
        var result = await _sut.HandleAsync(query, default);

        // Assert
        result.Should().BeFailure()
            .HaveErrorCode(ErrorCodes.NotFound);
    }

    [Fact]
    public async Task HandleAsync_DeleteBoss_ShouldReturnSpecificError()
    {
        // Arrange
        var command = new DeleteEmployeeCommand(1);

        // Act
        var result = await _sut.HandleAsync(command, default);

        // Assert
        result.Should().BeFailure()
            .HaveErrorCode(EmployeeErrorCodes.CannotDeleteBoss);
            
        result.Should().HaveError()
            .HaveMessage("The primary administrator (The Boss) cannot be deleted from the system.");
    }

    [Fact]
    public async Task HandleAsync_UpdateEmployee_ShouldReturnSuccessWithMetadata()
    {
        // Arrange
        var command = new UpdateEmployeeCommand(2, "Jane Updated", "Senior PM");

        // Act
        var result = await _sut.HandleAsync(command, default);

        // Assert
        result.Should().BeSuccess()
            .HaveMetadata("Audit.Timestamp");
        
        // Clean data access in tests
        var dto = result.ValueOrThrow();
        Assert.Equal("Jane Updated", dto.DisplayName);
    }

    [Fact]
    public async Task HandleAsync_UpdateBossToPM_ShouldFailAndThrowOnValueAccess()
    {
        // Arrange
        var command = new UpdateEmployeeCommand(1, "The Boss", "Product Manager"); // Demotion business rule

        // Act
        var result = await _sut.HandleAsync(command, default);

        // Assert
        result.Should().BeFailure()
            .HaveErrorCode(ErrorCodes.Conflict);

        // Demonstrate ThrowOnValueAccess for safety checks in tests
        result.Should().ThrowOnValueAccess<InvalidOperationException>();
    }

    [Fact]
    public async Task CreateEmployeeValidator_WhenInputIsInvalid_ShouldReturnValidationErrors()
    {
        // Arrange
        var validator = new CreateEmployeeValidator();
        var command = new CreateEmployeeCommand("", ""); // Invalid name and position

        // Act
        var result = await validator.ValidateAsync(command, default);

        // Assert
        result.Should().BeFailure()
            .HaveErrorCode(ErrorCodes.Validation);

        result.Should()
            .HaveValidationErrorFor("FullName").Contain("required").And
            .HaveValidationErrorFor("Position").Contain("required");
    }

    [Fact]
    public async Task CreateEmployeeValidator_WhenNameIsTooShortAndNoSpace_ShouldReturnMultipleErrorsForField()
    {
        // Arrange
        var validator = new CreateEmployeeValidator();
        var command = new CreateEmployeeCommand("Jo", "Developer"); // Too short and no space

        // Act
        var result = await validator.ValidateAsync(command, default);

        // Assert
        result.Should()
            .HaveValidationErrorFor("FullName")
                .Contain("too short")
                .Contain("space")
                .ContainAll("short", "name");
    }
}
