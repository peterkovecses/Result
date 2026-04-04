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
            .HaveError(ErrorCodes.NotFound);
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
            .HaveError(EmployeeErrorCodes.CannotDeleteBoss)
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
            .HaveError(ErrorCodes.Conflict);

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
            .HaveError("FullName").And
            .HaveError("Position");

        result.Should()
            .HaveValidationErrorFor("FullName").Contain("required").And
            .HaveValidationErrorFor("Position").Contain("required");
    }

    [Fact]
    public async Task CreateEmployeeAggregatedValidator_WithAggregatedValidation_ShouldAllowChaining()
    {
        // Arrange
        var validator = new CreateEmployeeAggregatedValidator();
        var command = new CreateEmployeeCommand("J", ""); // Too short name, empty position

        // Act
        var result = await validator.ValidateAsync(command, default);

        // Assert - Demonstrate chaining with HaveValidationProperty
        result.Should().BeFailure()
            .HaveError(ErrorCodes.Validation)
            .HaveValidationProperty("FullName")
                .Contain("too short")
                .And
            .HaveValidationProperty("Position")
                .Contain("required");
    }

    [Fact]
    public async Task HandleAsync_BulkUpdate_WhenAllSucceed_ShouldReturnSuccess()
    {
        // Arrange
        var command = new BulkUpdatePositionCommand([1, 2], "Senior Officer");

        // Act
        var result = await _sut.HandleAsync(command, default);

        // Assert
        result.Should().BeSuccess();
    }

    [Fact]
    public async Task HandleAsync_BulkUpdate_WhenSomeFail_ShouldReturnFailureWithCombinedErrors()
    {
        // Arrange
        var command = new BulkUpdatePositionCommand([998, 999], "Senior Officer");

        // Act
        var result = await _sut.HandleAsync(command, default);

        // Assert
        result.Should().BeFailure()
            .HaveErrors()
                .HaveCount(2)
                .Contain(e => e.Code == ErrorCodes.NotFound && e.Message == "Employee 998 not found.")
                .Contain(e => e.Code == ErrorCodes.NotFound && e.Message == "Employee 999 not found.");
    }

    [Fact]
    public async Task HandleAsync_GetSummary_ShouldReturnFormattedString()
    {
        // Arrange
        var createResult = await _sut.HandleAsync(new CreateEmployeeCommand("Test User", "Tester"), default);
        var employeeId = createResult.ValueOrThrow().Id;

        var query = new GetEmployeeSummaryQuery(employeeId);

        // Act
        var result = await _sut.HandleAsync(query, default);

        // Assert
        result.Should().BeSuccess()
            .HaveData(s => s is not null 
                           && s.Contains("Test User") 
                           && s.Contains("Tester"));
    }

    [Fact]
    public async Task HandleAsync_GetEmployee_WithOptionalAccess_ShouldDemonstrateValueOrDefault()
    {
        // Arrange
        var query = new GetEmployeeQuery(999);

        // Act
        var result = await _sut.HandleAsync(query, default);

        // Assert
        var employee = result.ValueOrDefault();
        
        Assert.Null(employee);
    }

    [Fact]
    public async Task CreateEmployeeValidator_ShouldDemonstrateCustomAssertionWithExtractMessages()
    {
        // Arrange
        var validator = new CreateEmployeeValidator();
        var command = new CreateEmployeeCommand("Jo", ""); // Too short and empty position

        // Act
        var result = await validator.ValidateAsync(command, default);

        // Assert
        result.Should().BeFailure();
        
        var error = result.Errors!.First(e => e.Code == "FullName");
        var messages = ValidationAssertions<ResultAssertions>.ExtractMessages(error, "FullName");
        
        Assert.Contains(messages, m => m.Contains("too short", StringComparison.OrdinalIgnoreCase));
    }
}
