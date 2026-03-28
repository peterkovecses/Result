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

// 3. Failure (using implicit conversion from Error)
public Result<Employee> Get(int id) => Error.NotFound($"Employee {id} not found.");
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
Beyond standard types, you can define business-specific error codes to help clients handle specific scenarios.

### Defining Custom Codes
```csharp
public static class UserErrorCodes
{
    public const string AccountDisabled = "User.AccountDisabled";
    public const string EmailNotVerified = "User.EmailNotVerified";
}
```

### Using Custom Codes
When creating an error, pass your custom code. It will be included in the `Code` property of the response.

```csharp
if (user.IsDisabled)
{
    return Error.Failure(
        message: "Your account is disabled.",
        code: UserErrorCodes.AccountDisabled);
}
```

This allows clients to check the `code` property in the JSON response and trigger specific UI logic.
