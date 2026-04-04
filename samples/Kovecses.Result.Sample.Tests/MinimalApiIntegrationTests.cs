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
}
