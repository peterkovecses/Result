# Kovecses.Result

[![NuGet Version](https://img.shields.io/nuget/v/Kovecses.Result?style=flat-square&logo=nuget)](https://www.nuget.org/packages/Kovecses.Result)
[![NuGet Version (AspNetCore)](https://img.shields.io/nuget/v/Kovecses.Result.AspNetCore?style=flat-square&logo=nuget&label=AspNetCore)](https://www.nuget.org/packages/Kovecses.Result.AspNetCore)
[![NuGet Version (FluentAssertions)](https://img.shields.io/nuget/v/Kovecses.Result.FluentAssertions?style=flat-square&logo=nuget&label=FluentAssertions)](https://www.nuget.org/packages/Kovecses.Result.FluentAssertions)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg?style=flat-square)](https://opensource.org/licenses/MIT)

A lightweight, functional, and robust Result pattern implementation for .NET 8, 9, and 10 with seamless ASP.NET Core integration.

---

### Support the Project
If you find this library useful, please give it a **star** on GitHub! It helps more developers discover the project. ⭐

---

### Table of Contents
1. [Introduction](#1-introduction)
2. [Installation](#installation)
3. [Core Library (Kovecses.Result)](#2-core-library-kovecsesresult)
    - [Basic Usage](#basic-usage)
    - [Accessing Error Details](#accessing-error-details)
    - [Functional Extensions](#functional-extensions-railway-oriented-programming)
    - [Async Chaining](#async-chaining-task-extensions)
    - [Error Aggregation (Combine)](#error-aggregation-combine)
    - [Custom Errors & Metadata](#custom-errors--metadata)
    - [Safety Helpers](#safety-helpers)
    - [JSON Serialization](#json-serialization-support)
    - [Advanced Usage (Performance)](#advanced-usage-performance)
4. [ASP.NET Core Integration](#3-aspnet-core-integration-kovecsesresultaspnetcore)
5. [Testing Support (Fluent Assertions)](#4-testing-support-kovecsesresultfluentassertions)

---

## Installation

Install the packages via NuGet:

```bash
# Core Library
dotnet add package Kovecses.Result

# ASP.NET Core Integration
dotnet add package Kovecses.Result.AspNetCore

# Fluent Assertions for Testing
dotnet add package Kovecses.Result.FluentAssertions
```

---

## 1. Introduction
The Result pattern encapsulates the outcome of an operation. Instead of relying on exceptions for flow control, methods return a `Result` object that explicitly indicates success or failure.

**Why use this library?**
- **High Performance:** Optimized generic factories for MediatR-style pipelines and no expensive exception overhead.
- **Type Safety:** Flat, unified error structure avoiding key collisions in validation.
- **Standard Compliant:** Native support for RFC 7807 (Problem Details) and `ValidationProblemDetails`.
- **Railway-Oriented:** Build clean, declarative processing pipelines using `Match`, `Map`, and `Bind`.

---

## 2. Core Library (`Kovecses.Result`)

The core library contains the fundamental types and functional extensions. It supports multiple errors per result and implicit conversions.

### Basic Usage
The library supports powerful implicit conversions to reduce boilerplate, including support for C# 12 collection expressions.

```csharp
// Success (with or without data)
public Result Create() => Result.Success();
public Result<Employee> Get() => new Employee(1, "John Doe", "Engineer"); // Implicit conversion

// Failure (Single Error)
public Result<Employee> Get(int id) => Error.NotFound($"Employee {id} not found."); // Implicit conversion

// Failure (Custom code and message - defaults to ErrorType.Failure / HTTP 400)
public Result Process() => Result.Failure("Order.InvalidState", "The order cannot be modified in its current state.");

// Failure (With explicit type - e.g. Conflict -> HTTP 409)
public Result Create() => Result.Failure("User.Exists", "User already registered.", ErrorType.Conflict);

// Failure with data and explicit type (e.g. NotFound -> HTTP 404)
public Result<User> GetUser(int id) 
    => Result.Failure<User>("User.NotFound", $"User {id} not found.", ErrorType.NotFound);
```

#### Multiple Errors
The library natively supports returning multiple errors at once.

```csharp
// Failure (Multiple Errors using Collection Expressions - C# 12)
public Result Validate(User user) => [
    Error.Validation("Email", "Email is required."),
    Error.Validation("Age", "Must be 18 or older.")
]; // Implicitly converts Error[] to Result

// Failure (List of Errors)
public Result<Employee> RegisterEmployee(RegisterRequest request) {
    List<Error> errors = [];
    if (string.IsNullOrEmpty(request.Name)) errors.Add(Error.Validation("Name", "Required"));
    if (request.Salary < 0) errors.Add(Error.Validation("Salary", "Positive only"));

    if (errors.Count > 0) 
        return errors; // Implicitly converts List<Error> to Result<Employee>

    return new Employee(request.Name, request.Salary);
}
```

### Accessing Error Details
When an operation fails, you can easily inspect the results:

```csharp
var result = Get(123);
if (result.IsFailure) 
{
    Error? first = result.FirstError;            // The primary Error object
    Error[]? all = result.Errors;                // All Error objects
    string? msg = result.FirstErrorMessage;      // Message of the primary error
    string summary = result.JoinErrorMessages(); // All messages joined: "Msg 1; Msg 2"
}
```

### Functional Extensions (Railway-Oriented Programming)
Reduce nested `if` statements and build declarative pipelines.

```csharp
// Match: Execute different paths based on state (supports both single error and error array)
return result.Match(
    data => CreatedAtAction(nameof(Get), new { id = data.Id }, data),
    (Error err) => result.ToActionResult() // Single error (most common)
);

return result.Match(
    data => CreatedAtAction(nameof(Get), new { id = data.Id }, data),
    (Error[] errors) => result.ToActionResult() // All errors
);

// Map: Transform success data
Result<UserDto> dto = result.Map(u => new UserDto(u.Id, u.Name));

// Bind: Chain result-returning operations (FlatMap)
return await GetUserAsync(id)
    .BindAsync(user => UpdateAsync(user));

// Tap: Execute side effects (e.g., logging) without modifying the result
result.Tap(data => Console.WriteLine($"Processed: {data}"));
```

### Async Chaining (Task Extensions)
Chain operations directly on `Task<Result>` without manual `await` at each step. This makes asynchronous "railway-oriented" pipelines much cleaner.

```csharp
return await _repository.GetByIdAsync(id)               // Task<Result<User>>
    .BindAsync(user => _service.Validate(user))         // Task<Result<User>>
    .BindAsync(user => _repository.Update(user))        // Task<Result>
    .MatchAsync(() => Results.NoContent(), errors => Result.Failure(errors).ToMinimalApiResult());
```

### Error Aggregation (`Combine`)
Merges multiple results into one. If any fail, it aggregates **all** errors from all results into a single flat collection. This avoids any information loss and is perfect for complex validation scenarios.

```csharp
var result = Result.Combine(
    ValidateEmail(email),
    ValidatePassword(password),
    CheckPermissions(user)
);

if (result.IsFailure) {
    // result.Errors contains all collected errors from all failing steps
}
```

### Custom Errors & Metadata
Define domain-specific errors and attach extra context to any result.

```csharp
// 1. Define constants for your business error codes
public static class UserErrorCodes {
    public const string Disabled = "User.Disabled";
}

// 2. Create a factory for domain-specific Error objects
public static class UserErrors {
    public static Error Disabled(int id) => Error.Disabled("User is disabled.", UserErrorCodes.Disabled);
}
```
// Attach Success Metadata
var metadata = new Dictionary<string, object> { { "TraceId", "abc-123" } };
return Result.Success(data, metadata);

// Validation errors with field-level metadata (recommended pattern)
// Metadata keys are property names, values are validation message arrays
var validationMetadata = new Dictionary<string, object>
{
    ["Email"] = new[] { "Email is required.", "Email format is invalid." },
    ["Password"] = new[] { "Password must be at least 8 characters." }
};

var validationError = Error.Validation(
    ErrorCodes.Validation,
    "Validation failed.",
    validationMetadata
);

var result = Result.Failure(validationError);
```

**Validation Error Response Format:**
```json
{
  "data": null,
  "errors": [
    {
      "code": "General.Validation",
      "message": "Validation failed.",
      "type": 2,
      "metadata": {
        "Email": [
          "Email is required.",
          "Email format is invalid."
        ],
        "Password": [
          "Password must be at least 8 characters."
        ]
      }
    }
  ]
}
```

This pattern is recommended for handling multiple validation errors in application layers (e.g., MediatR ValidationBehavior). Clients can easily parse and display field-level errors directly from metadata without additional processing.


### Safety Helpers
Fail fast or provide fallbacks when certain of the outcome.
```csharp
var data = result.ValueOrThrow(); // Throws InvalidOperationException on failure with all error details
var data = result.ValueOrThrow(errors => new MyException(errors[0].Message));
var data = result.ValueOrDefault("Fallback"); 
```

### JSON Serialization Support

The library includes built-in JSON converters for `System.Text.Json` that handle both serialization and deserialization correctly.

```csharp
// Serialization
var result = Result.Success(new UserDto(1, "John"));
var json = JsonSerializer.Serialize(result);

// Deserialization (works with direct API responses)
var deserialized = JsonSerializer.Deserialize<Result<UserDto>>(json);

// Works with naming policies
var options = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
var restored = JsonSerializer.Deserialize<Result<UserDto>>(json, options);
```

### Advanced Usage (Performance)
The library is optimized for high-performance scenarios like MediatR Pipelines. The `CreateFailure<TResponse>` method uses cached delegates to instantiate generic result types nearly as fast as direct constructor calls.

```csharp
// Example in a MediatR Pipeline Behavior
public async Task<TResponse> Handle(TRequest req, RequestHandlerDelegate<TResponse> next, CancellationToken ct) {
    var errors = await ValidateAsync(req);
    if (errors.Any()) {
        return Result.CreateFailure<TResponse>(errors); // Ultra-fast generic instantiation
    }
    return await next();
}
```

---

## 3. ASP.NET Core Integration (`Kovecses.Result.AspNetCore`)

Standardized HTTP responses with zero effort.

### Automatic Mapping
The library detects the nature of failures and responds accordingly:
- **Pure Validation:** If all errors are of type `Validation`, it returns `400 ValidationProblemDetails` (standard ASP.NET format).
- **Mixed/Other Failures:** Uses "First Error Wins" for status code and includes all errors in the `extensions["errors"]` field.

| ErrorType | Status Code | Default Title | Description |
|-----------|-------------|---------------|-------------|
| Validation | 400 | Validation Error | Grouped into `ValidationProblemDetails` |
| Failure | 400 | Bad Request | General business rule violations |
| NotFound | 404 | Not Found | Resource does not exist |
| Conflict | 409 | Conflict | Resource state conflict (e.g., duplicate) |
| Unauthorized| 401 | Unauthorized | Authentication required |
| Forbidden | 403 | Forbidden | Insufficient permissions |
| Timeout | 408 | Request Timeout | The operation timed out |
| Canceled | 400 | Bad Request | Operation was canceled |
| Unexpected| 500 | Internal Server Error | Unhandled or internal errors |

### Mapping Strategies

#### Standard REST (Public APIs)
Returns the data on success (`200 OK` or `204 No Content`) and `ProblemDetails` on failure.
```csharp
// Minimal API Example
app.MapGet("/users/{id}", (int id, IMediator m) => m.Send(new GetUser(id)).ToMinimalApiResult());

// Controller Example
public IActionResult Create(User cmd) => _service.Create(cmd).ToActionResult();
```

#### Wrapped Results (Internal/Typed Clients)
Returns the full `Result` object in the body (e.g., for Blazor or Typed Clients).
```csharp
// Server-side
return result.ToMinimalApiResult(includeResultInResponse: true);

// Client-side deserialization
var result = await response.Content.ReadFromJsonAsync<Result<UserDto>>();
if (result.IsSuccess)
    Console.WriteLine(result.Data.Name);
```

#### Direct Error Mapping
You can also map a single `Error` object directly without wrapping it in a `Result` first.
```csharp
Error error = Error.NotFound("User not found");
return error.ToMinimalApiResult(); // Returns 404 ProblemDetails
return error.ToActionResult();     // Returns 404 ObjectResult(ProblemDetails)
```

---

## 4. Testing Support (`Kovecses.Result.FluentAssertions`)

Fluent extension methods for readable and maintainable tests.

```csharp
using Kovecses.Result.FluentAssertions;

[Fact]
public void Test_Operation()
{
    var result = _service.DoWork();
    
    // Assert Success
    result.Should().BeSuccess()
        .HaveData(u => u.Name == "John");
        
    // Assert Specific Error
    result.Should().BeFailure()
        .HaveError(ErrorCodes.NotFound)
            .HaveMessage("User not found.");
            
    // Assert Collection of Errors
    result.Should().HaveErrors()
        .HaveCount(2)
        .AllBeOfType(ErrorType.Validation).And
        .Contain(e => e.Code == "Email");
        
    // Assert Field-Specific Validation
    result.Should().HaveValidationErrorFor("Email").Contain("required");

    // Assert detailed Error properties and chain back to Result
    result.Should()
        .HaveError("User.Disabled")
            .HaveMessage("User is disabled.").And
        .BeFailure();

    // Assert that accessing data on a failure result throws the correct exception
    result.Should().ThrowOnValueAccess<InvalidOperationException>();
}
```

### Aggregated Validation Errors with Metadata

When validating multiple fields, create a single aggregated validation error with metadata containing field-level messages:

```csharp
public async Task<Result> ValidateAsync(CreateUserCommand request)
{
    var validationMessages = new Dictionary<string, object>();
    
    if (string.IsNullOrEmpty(request.FullName))
        validationMessages["FullName"] = new[] { "Name is required." };
    
    if (request.Position is not { Length: >= 3 })
        validationMessages["Position"] = new[] { "Position must be at least 3 characters." };
    
    if (validationMessages.Count > 0)
        return Result.Failure(Error.Validation(ErrorCodes.Validation, "Validation failed.", validationMessages));
    
    return Result.Success();
}
```

Assert with chainable fluent syntax:

```csharp
[Fact]
public async Task CreateUser_WithValidationFailures_ShouldAggregateErrors()
{
    var result = await _validator.ValidateAsync(new CreateUserCommand 
    { 
        FullName = "", 
        Position = "PM"  // Too short
    });
    
    result.Should().BeFailure()
        .HaveError(ErrorCodes.Validation)
        .HaveValidationProperty("FullName")
            .Contain("required")
            .And
        .HaveValidationProperty("Position")
            .Contain("at least 3 characters");
}
```

`HaveValidationProperty()` returns a chainable `ValidationPropertyAssertions` with:
- `.Contain(string message)` - Assert message contains text
- `.ContainAll(params string[] messages)` - Assert all messages are present
- `.HaveCount(int expected)` - Assert exact message count
- `.BeExactly(params string[] messages)` - Assert exact match
- `.And` - Chain back to error assertions for multiple property checks

### Custom Assertions
If you need to write custom assertions for validation errors, you can use the `ValidationAssertions.ExtractMessages` helper to retrieve and normalize error messages from an `Error` object regardless of how they are stored (JsonElement, List, etc.).

```csharp
var error = result.Errors!.First(e => e.Code == "Email");
var messages = ValidationAssertions<ResultAssertions>.ExtractMessages(error, "Email");

Assert.Contains(messages, m => m.Contains("required"));
```
