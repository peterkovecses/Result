using System.Text.Json;
using Kovecses.Result.FluentAssertions;
using Kovecses.Result.Sample.Core;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace Kovecses.Result.Sample.Tests;

/// <summary>
/// Integration tests for the Minimal API to demonstrate Kovecses.Result.FluentAssertions in integration scenarios.
/// </summary>
public class MinimalApiIntegrationTests(WebApplicationFactory<Program> factory) : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client = factory.CreateClient();

    [Fact]
    public async Task GetEmployeeWrapped_Existing_ShouldReturnSuccessfulResultObject()
    {
        // Act
        var response = await _client.GetAsync("/employees/1/wrapped");

        // Assert
        response.EnsureSuccessStatusCode();
        
        // We expect the body to be a serialized Result<Employee> object
        var result = await response.Content.ReadFromJsonAsync<Result<Employee>>();
        
        Assert.NotNull(result);
        result.Should().BeSuccess();
        
        // Using ValueOrThrow in tests is a clean way to get data after assertion
        var employee = result.ValueOrThrow();
        Assert.Equal("The Boss", employee.FullName);
    }

    [Fact]
    public async Task GetEmployeeWrapped_NonExisting_ShouldReturnFailureResultObject()
    {
        // Act
        var response = await _client.GetAsync("/employees/999/wrapped");

        // Assert
        // The API returns 404 but the body is a Result object because we used /wrapped
        Assert.Equal(System.Net.HttpStatusCode.NotFound, response.StatusCode);
        
        var result = await response.Content.ReadFromJsonAsync<Result<Employee>>();
        
        Assert.NotNull(result);
        result.Should().BeFailure()
            .HaveError(ErrorCodes.NotFound)
                .HaveMessage("Employee 999 not found.");
    }

    [Fact]
    public async Task PostEmployee_WithInvalidData_ShouldReturnValidationProblemWithMetadata()
    {
        // Arrange
        var command = new CreateEmployeeCommand("", ""); // Both empty

        // Act
        var response = await _client.PostAsJsonAsync("/employees/validate-metadata", command);

        // Assert
        Assert.Equal(System.Net.HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);

        var problem = await response.Content.ReadFromJsonAsync<JsonElement>();
        var errors = problem.GetProperty("errors");

        // Metadata keys should be present as field names
        Assert.True(errors.TryGetProperty("FullName", out var nameErrors));
        Assert.Contains("Name is required.", nameErrors.EnumerateArray().Select(e => e.GetString()));

        Assert.True(errors.TryGetProperty("Position", out var positionErrors));
        Assert.Contains("Position is required.", positionErrors.EnumerateArray().Select(e => e.GetString()));
        
        // Generic code should NOT be a key in errors
        Assert.False(errors.TryGetProperty(ErrorCodes.Validation, out _));
    }
}
