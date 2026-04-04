using System.Text.Json;
using Kovecses.Result;

Console.WriteLine("=== Kovecses.Result Serialization Test ===\n");

// Test DTOs
var userDto = new UserDto(1, "John Doe", "john@example.com");

// Different serializer options to test
var defaultOptions = new JsonSerializerOptions { WriteIndented = true };
var camelCaseOptions = new JsonSerializerOptions 
{ 
    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
    WriteIndented = true 
};
var webOptions = new JsonSerializerOptions(JsonSerializerDefaults.Web) 
{ 
    WriteIndented = true 
};

Console.WriteLine("========================================");
Console.WriteLine("TEST 1: Success Result<T> Serialization");
Console.WriteLine("========================================\n");

var successResult = Result.Success(userDto);
TestSerialization("Default Options", successResult, defaultOptions);
TestSerialization("CamelCase Options", successResult, camelCaseOptions);
TestSerialization("Web Defaults", successResult, webOptions);

Console.WriteLine("\n========================================");
Console.WriteLine("TEST 2: Failure Result<T> Serialization");
Console.WriteLine("========================================\n");

var failureResult = Result.Failure<UserDto>(
[
    Error.Validation("Email", "Email is required"),
    Error.NotFound("User not found")
]);
TestSerialization("Default Options", failureResult, defaultOptions);
TestSerialization("CamelCase Options", failureResult, camelCaseOptions);
TestSerialization("Web Defaults", failureResult, webOptions);

Console.WriteLine("\n========================================");
Console.WriteLine("TEST 3: Non-generic Result Serialization");
Console.WriteLine("========================================\n");

var simpleSuccess = Result.Success();
var simpleFailure = Result.Failure(Error.Unauthorized("Access denied"));
TestSerialization("Simple Success", simpleSuccess, camelCaseOptions);
TestSerialization("Simple Failure", simpleFailure, camelCaseOptions);

Console.WriteLine("\n========================================");
Console.WriteLine("TEST 4: Error with Metadata");
Console.WriteLine("========================================\n");

var errorWithMetadata = Error.Custom(
    "User.Locked",
    "Account is locked",
    ErrorType.Forbidden,
    new Dictionary<string, object>
    {
        { "lockReason", "Too many failed attempts" },
        { "unlockTime", DateTime.UtcNow.AddHours(1).ToString("O") },
        { "attemptCount", 5 }
    }
);
var resultWithMetadata = Result.Failure<UserDto>(errorWithMetadata);
TestSerialization("Error with Metadata", resultWithMetadata, camelCaseOptions);

Console.WriteLine("\n========================================");
Console.WriteLine("TEST 5: Cross-Option Deserialization");
Console.WriteLine("========================================\n");

// Serialize with one option, deserialize with another
var json = JsonSerializer.Serialize(successResult, camelCaseOptions);
Console.WriteLine("Serialized with CamelCase:");
Console.WriteLine(json);
Console.WriteLine();

// Try deserializing with different options
Console.WriteLine("Deserialize with Default Options:");
var result1 = JsonSerializer.Deserialize<Result<UserDto>>(json, defaultOptions);
PrintResultGeneric(result1);

Console.WriteLine("Deserialize with Web Defaults:");
var result2 = JsonSerializer.Deserialize<Result<UserDto>>(json, webOptions);
PrintResultGeneric(result2);

Console.WriteLine("\n========================================");
Console.WriteLine("TEST 6: Raw JSON Deserialization (Simulating External API)");
Console.WriteLine("========================================\n");

// Simulate JSON coming from an external API (camelCase)
var externalApiJson = """
{
    "data": {
        "id": 42,
        "name": "External User",
        "email": "external@api.com"
    },
    "errors": null,
    "metadata": null
}
""";

Console.WriteLine("External API JSON (camelCase):");
Console.WriteLine(externalApiJson);
Console.WriteLine();

Console.WriteLine("Deserialize with Default Options:");
var external1 = JsonSerializer.Deserialize<Result<UserDto>>(externalApiJson, defaultOptions);
PrintResultGeneric(external1);

Console.WriteLine("Deserialize with CamelCase Options:");
var external2 = JsonSerializer.Deserialize<Result<UserDto>>(externalApiJson, camelCaseOptions);
PrintResultGeneric(external2);

Console.WriteLine("Deserialize with Web Defaults:");
var external3 = JsonSerializer.Deserialize<Result<UserDto>>(externalApiJson, webOptions);
PrintResultGeneric(external3);

// Simulate PascalCase JSON
var pascalCaseJson = """
{
    "Data": {
        "Id": 42,
        "Name": "Pascal User",
        "Email": "pascal@api.com"
    },
    "Errors": null,
    "Metadata": null
}
""";

Console.WriteLine("\nPascalCase JSON:");
Console.WriteLine(pascalCaseJson);
Console.WriteLine();

Console.WriteLine("Deserialize with Default Options:");
var pascal1 = JsonSerializer.Deserialize<Result<UserDto>>(pascalCaseJson, defaultOptions);
PrintResultGeneric(pascal1);

Console.WriteLine("Deserialize with Web Defaults:");
var pascal2 = JsonSerializer.Deserialize<Result<UserDto>>(pascalCaseJson, webOptions);
PrintResultGeneric(pascal2);

Console.WriteLine("\n========================================");
Console.WriteLine("TEST 7: Failure JSON Deserialization");
Console.WriteLine("========================================\n");

var failureJson = """
{
    "data": null,
    "errors": [
        {
            "code": "User.NotFound",
            "message": "User with ID 123 was not found",
            "type": 4,
            "metadata": null
        },
        {
            "code": "General.Validation",
            "message": "Email format is invalid",
            "type": 2,
            "metadata": {
                "field": "email",
                "attemptedValue": "not-an-email"
            }
        }
    ],
    "metadata": {
        "requestId": "abc-123",
        "timestamp": "2024-01-15T10:30:00Z"
    }
}
""";

Console.WriteLine("Failure JSON:");
Console.WriteLine(failureJson);
Console.WriteLine();

Console.WriteLine("Deserialize with CamelCase Options:");
var failureDeserialized = JsonSerializer.Deserialize<Result<UserDto>>(failureJson, camelCaseOptions);
PrintResultGeneric(failureDeserialized);

Console.WriteLine("\n========================================");
Console.WriteLine("TEST 8: Check for $type in output");
Console.WriteLine("========================================\n");

var checkJson = JsonSerializer.Serialize(failureResult, camelCaseOptions);
Console.WriteLine("Serialized failure result:");
Console.WriteLine(checkJson);
Console.WriteLine();
Console.WriteLine($"Contains '$type': {checkJson.Contains("$type")}");

Console.WriteLine("\n=== All Tests Complete ===");

// Helper methods
static void TestSerialization<T>(string testName, T value, JsonSerializerOptions options)
{
    Console.WriteLine($"--- {testName} ---");
    
    try
    {
        var json = JsonSerializer.Serialize(value, options);
        Console.WriteLine($"Serialized:\n{json}");
        
        var deserialized = JsonSerializer.Deserialize<T>(json, options);
        Console.WriteLine($"Deserialized successfully: {deserialized is not null}");
        
        if (deserialized is Result<UserDto> resultT)
        {
            PrintResultGeneric(resultT);
        }
        else if (deserialized is Result result)
        {
            PrintResultBase(result);
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"ERROR: {ex.Message}");
    }
    
    Console.WriteLine();
}

static void PrintResultBase(Result? result)
{
    if (result is null)
    {
        Console.WriteLine("  Result: NULL");
        return;
    }
    
    Console.WriteLine($"  IsSuccess: {result.IsSuccess}");
    Console.WriteLine($"  IsFailure: {result.IsFailure}");
    Console.WriteLine($"  Errors: {(result.Errors is null ? "null" : $"Count={result.Errors.Count}")}");
    
    if (result.Errors is not null)
    {
        foreach (var error in result.Errors)
        {
            Console.WriteLine($"    - [{error.Type}] {error.Code}: {error.Message}");
            if (error.Metadata is not null)
            {
                foreach (var kvp in error.Metadata)
                {
                    Console.WriteLine($"      Metadata[{kvp.Key}]: {kvp.Value} ({kvp.Value?.GetType().Name})");
                }
            }
        }
    }
    
    Console.WriteLine($"  Metadata: {(result.Metadata is null ? "null" : $"Count={result.Metadata.Count}")}");
    if (result.Metadata is not null)
    {
        foreach (var kvp in result.Metadata)
        {
            Console.WriteLine($"    - {kvp.Key}: {kvp.Value} ({kvp.Value?.GetType().Name})");
        }
    }
}

static void PrintResultGeneric(Result<UserDto>? result)
{
    PrintResultBase(result);
    
    if (result is not null)
    {
        Console.WriteLine($"  Data: {(result.Data is null ? "null" : $"Id={result.Data.Id}, Name={result.Data.Name}, Email={result.Data.Email}")}");
    }
}

// Test DTO
public record UserDto(int Id, string Name, string Email);