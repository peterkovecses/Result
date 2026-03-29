# Kovecses.Result

A lightweight, robust, and ASP.NET Core-integrated Result pattern implementation for .NET 8, 9, and 10.

## 1. Introduction
`Kovecses.Result` library is designed to implement the Result pattern in .NET applications. It provides a consistent way to handle operation outcomes (success or failure) across all layers of your application, with seamless integration into ASP.NET Core Minimal APIs and Controllers.

## 2. What is the Result Pattern?
The Result pattern is a design pattern that encapsulates the outcome of an operation. Instead of relying on exceptions for flow control, a method returns a `Result` object that explicitly indicates whether the operation succeeded or failed, along with any relevant data or error information.

### Why use Result instead of Exceptions?
- **Performance:** Throwing and catching exceptions is a heavy operation for the runtime. Using Results is as fast as any other object return.
- **Explicitness:** Method signatures like `public Result<User> GetUser(int id)` clearly communicate possible failure, making the API more self-documenting.
- **Predictability:** Exceptions can be thrown from anywhere, often leading to unhandled crashes. Results force the caller to consider the failure path.
- **Clean Flow:** It enables a "railway-oriented" programming style where you can flow results through multiple layers without complex `try-catch` blocks.

## 3. Usage Patterns

### Creating Success and Failure
The library supports generic and non-generic results with implicit conversions for clean code.

```csharp
// 1. Success without data
public Result Update() => Result.Success();

// 2. Success with data (using implicit conversion)
public Result<Employee> Get() => new Employee(1, "John Doe", "Engineer");

// 3. Failure using built-in factories (implicit conversion from Error)
public Result<Employee> Get(int id) => Error.NotFound($"Employee {id} not found.");

// 4. Other Built-in Error Factories
public Result Create(string name) => Error.Conflict($"Employee {name} already exists.");
public Result Validate(string email) => Error.Validation(new Dictionary<string, object> { { "Email", "Invalid format" } });
public Result Delete(int id) => Error.Failure("Cannot delete the administrator.", "Admin.DeleteNotAllowed");
public Result CheckAuth() => Error.Unauthorized("Please log in.");
```

### Mapping to HTTP Responses
The `Kovecses.Result.AspNetCore` package provides extension methods to automatically map results to `IResult` (Minimal API) or `IActionResult` (Controllers).

#### Standard REST (Public APIs)
Ideal for public-facing APIs where you want to follow standard REST conventions: returning the data on success (`200 OK`) and `ProblemDetails` on failure.

```csharp
// Minimal API
app.MapGet("/employees/{id}", (int id, IEmployeeService service) => 
    service.GetEmployee(id).ToMinimalApiResult());

// Controller
[HttpGet("{id}")]
public IActionResult GetEmployee(int id) => 
    _service.GetEmployee(id).ToActionResult();
```

#### Wrapped Results (Internal/Typed Clients)
Useful for internal communication where the client (e.g., a Blazor app) knows the `Result` structure.

```csharp
// Returns: { "isSuccess": true, "data": { ... }, "error": null }
return result.ToMinimalApiResult(includeResultInResponse: true);
```

## 4. Automatic HTTP Response Generation
The library automatically maps `ErrorType` to the most appropriate HTTP status code and generates a standardized `ProblemDetails` response.

| ErrorType | Status Code | Description |
|-----------|-------------|-------------|
| Failure | 400 | General business rule violation |
| Validation| 400 | Input validation errors (includes metadata) |
| NotFound | 404 | Resource does not exist |
| Conflict | 409 | Resource state conflict (e.g., duplicate) |
| Unauthorized| 401 | Authentication required |
| Forbidden | 403 | Insufficient permissions |
| Timeout | 408 | Request timeout |
| Unexpected| 500 | Internal server error |

## 5. Custom Error Codes
Beyond standard types, you can define business-specific error codes and factory methods.

### Defining Custom Codes
```csharp
public static class UserErrorCodes
{
    public const string AccountDisabled = "User.AccountDisabled";
    public const string EmailNotVerified = "User.EmailNotVerified";
}
```

### Extension Methods for Custom Errors
You can create extension methods for the `Error` class to provide a clean, fluent API for your domain-specific errors.

```csharp
public static class UserErrors
{
    public static Error AccountDisabled(int userId) => Error.Failure(
        message: $"Account {userId} is disabled.",
        code: UserErrorCodes.AccountDisabled);

    public static Error EmailNotVerified() => Error.Unauthorized(
        message: "Your email is not verified.",
        code: UserErrorCodes.EmailNotVerified);
}

// Usage in Service:
if (user.IsDisabled) return UserErrors.AccountDisabled(user.Id);
```

## 6. Fluent Assertions for Testing
The `Kovecses.Result.FluentAssertions` package provides a set of fluent extension methods for xUnit to make your tests more readable.

### Basic Assertions
```csharp
using Kovecses.Result.FluentAssertions;

[Fact]
public void Should_Be_Successful()
{
    var result = _service.DoWork();
    
    result.Should().BeSuccess();
}

[Fact]
public void Should_Fail_With_Specific_Error()
{
    var result = _service.Get(999);
    
    result.Should().BeFailure()
        .HaveErrorCode(ErrorCodes.NotFound);
        
    result.Should().HaveError()
        .HaveMessage("Employee 999 not found.");
}
```

### Generic Result Assertions
```csharp
[Fact]
public void Should_Have_Correct_Data()
{
    var result = _service.Get(1);
    
    result.Should().BeSuccess()
        .HaveData(e => e.FullName == "The Boss");
}
```

### Why use these assertions?
- **Readability:** The assertions follow the `Should().Be...` pattern, making tests feel like natural language.
- **Improved Stack Traces:** The library uses `[StackTraceHidden]`, so when an assertion fails, the IDE points directly to your test method instead of the library's internal code.
- **Chaining:** You can chain multiple assertions for complex objects.
