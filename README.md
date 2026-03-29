# Kovecses.Result

[![NuGet Version](https://img.shields.io/nuget/v/Kovecses.Result?style=flat-square&logo=nuget)](https://www.nuget.org/packages/Kovecses.Result)
[![NuGet Version (AspNetCore)](https://img.shields.io/nuget/v/Kovecses.Result.AspNetCore?style=flat-square&logo=nuget&label=AspNetCore)](https://www.nuget.org/packages/Kovecses.Result.AspNetCore)
[![NuGet Version (FluentAssertions)](https://img.shields.io/nuget/v/Kovecses.Result.FluentAssertions?style=flat-square&logo=nuget&label=FluentAssertions)](https://www.nuget.org/packages/Kovecses.Result.FluentAssertions)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg?style=flat-square)](https://opensource.org/licenses/MIT)

A lightweight, functional, and robust Result pattern implementation for .NET 8, 9, and 10 with seamless ASP.NET Core integration.

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
- **Performance:** No expensive exception throwing/catching for business logic.
- **Explicitness:** Method signatures clearly communicate potential failure.
- **Railway-Oriented:** Build clean, declarative processing pipelines using `Match`, `Map`, and `Bind`.
- **Predictability:** Forces the caller to consider the failure path, leading to fewer unhandled crashes.

---

## 2. Core Library (`Kovecses.Result`)

The core library contains the fundamental types and functional extensions for your domain and application layers. It supports **implicit conversions** for cleaner code.

### Basic Usage
```csharp
// Success (with or without data)
public Result Create() => Result.Success();
public Result<Employee> Get() => new Employee(1, "John Doe", "Engineer"); // Implicit conversion

// Failure (using built-in factories)
public Result<Employee> Get(int id) => Error.NotFound($"Employee {id} not found.");
public Result Create(string name) => Error.Conflict($"Employee {name} already exists.");
public Result Validate(string email) => Error.Validation(new Dictionary<string, object> { { "Email", "Invalid format" } });
```

### Functional Extensions (Railway-Oriented Programming)
Reduce nested `if` statements and build declarative pipelines.

```csharp
// Match: Execute different paths based on state
return result.Match(
    data => CreatedAtAction(nameof(Get), new { id = data.Id }, data),
    error => result.ToActionResult()
);

// Map: Transform success data
Result<UserDto> dto = result.Map(u => new UserDto(u.Id, u.Name));

// Bind: Chain result-returning operations (FlatMap)
return await GetUserAsync(id)
    .BindAsync(user => UpdateAsync(user));

// Tap: Execute side effects (e.g., logging) without modifying the result
result.Tap(data => Console.WriteLine($"Success: {data}"));
await result.TapAsync(async data => await LogToDb(data));
```

### Async Chaining (Task Extensions)
Chain operations directly on `Task<Result>` without manual `await` at each step. This makes asynchronous "railway-oriented" pipelines much cleaner.

```csharp
return await _repository.GetByIdAsync(id)               // Task<Result<User>>
    .BindAsync(user => _service.Validate(user))         // Task<Result<User>>
    .BindAsync(user => _repository.Update(user))        // Task<Result>
    .MatchAsync(() => Results.NoContent(), e => e.ToMinimalApiResult());
```

### Error Accumulation (`Combine`)
Aggregate multiple results into one. If any fail, it returns a validation error containing all failure information, merging metadata correctly.

```csharp
var result = Result.Combine(
    ValidateName(request.FullName),
    ValidatePosition(request.Position)
);

if (result.IsFailure) {
    // result.Error.Metadata contains all aggregated validation messages
}
```

### Custom Errors & Metadata
Define domain-specific errors and attach extra context to any result.

```csharp
// Define extensions for Error class
public static class UserErrors {
    public static Error Disabled(int id) => Error.Failure($"User {id} is disabled.", "User.Disabled");
}

// Attach Success Metadata
var metadata = new Dictionary<string, object> { { "TraceId", "abc-123" } };
return Result.Success(data, metadata);
```

### Safety Helpers
Fail fast or provide fallbacks when certain of the outcome.
```csharp
var data = result.ValueOrThrow(); // Throws InvalidOperationException on failure
var data = result.ValueOrThrow(e => new MyException(e.Message));
var data = result.ValueOrDefault("Fallback"); 
```

---

## 3. ASP.NET Core Integration (`Kovecses.Result.AspNetCore`)

Automatically map results to standardized HTTP responses.

### Automatic Mapping
The library generates a standardized `ProblemDetails` response by mapping `ErrorType` to HTTP status codes:

| ErrorType | Status Code | Description |
|-----------|-------------|-------------|
| Failure / Validation | 400 | General business rule or input validation errors |
| NotFound | 404 | Resource does not exist |
| Conflict | 409 | Resource state conflict (e.g., duplicate) |
| Unauthorized| 401 | Authentication required |
| Forbidden | 403 | Insufficient permissions |
| Timeout | 408 | Request timeout |
| Unexpected| 500 | Internal server error |

### Mapping Strategies

#### Standard REST (Public APIs)
Returns the data on success (`200 OK`) and `ProblemDetails` on failure.
```csharp
// Controller
public IActionResult Get(int id) => _service.Get(id).ToActionResult();

// Minimal API
app.MapGet("/users/{id}", (int id, IService s) => s.Get(id).ToMinimalApiResult());
```

#### Wrapped Results (Internal/Typed Clients)
Returns the full `Result` object in the body (e.g., for Blazor or Typed Clients).
```csharp
// Returns: { "isSuccess": true, "data": { ... }, "error": null, "metadata": { ... } }
return result.ToMinimalApiResult(includeResultInResponse: true);
```

---

## 4. Testing Support (`Kovecses.Result.FluentAssertions`)

A set of fluent extension methods for xUnit to make your tests more readable.

```csharp
using Kovecses.Result.FluentAssertions;

[Fact]
public void Test_Operation()
{
    var result = _service.DoWork();
    
    // Assert Success with data check
    result.Should().BeSuccess()
        .HaveData(u => u.Name == "John");
        
    // Assert Failure with specific code and message
    result.Should().BeFailure()
        .HaveErrorCode(ErrorCodes.NotFound);
        
    result.Should().HaveError()
        .HaveMessage("Not found.");
}
```

### Why use these assertions?
- **Readability:** Follows the `Should().Be...` pattern for natural language feel.
- **Improved Stack Traces:** Uses `[StackTraceHidden]`, so failure points directly to your test method instead of library internals.
- **Data Chaining:** Easily chain assertions for complex objects.
