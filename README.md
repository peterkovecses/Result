# Kovecses.Result

[![NuGet: Kovecses.Result](https://img.shields.io/nuget/v/Kovecses.Result?style=flat-square&logo=nuget)](https://www.nuget.org/packages/Kovecses.Result)
[![NuGet: Kovecses.Result.AspNetCore](https://img.shields.io/nuget/v/Kovecses.Result.AspNetCore?style=flat-square&logo=nuget&label=AspNetCore)](https://www.nuget.org/packages/Kovecses.Result.AspNetCore)
[![NuGet: Kovecses.Result.FluentAssertions](https://img.shields.io/nuget/v/Kovecses.Result.FluentAssertions?style=flat-square&logo=nuget&label=FluentAssertions)](https://www.nuget.org/packages/Kovecses.Result.FluentAssertions)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg?style=flat-square)](https://opensource.org/licenses/MIT)

Kovecses.Result provides `Result` and `Result<TData>` types for representing successful operations and failures with structured errors. The companion packages map results to ASP.NET Core responses and provide fluent assertions for tests. The packages target .NET 8, .NET 9, and .NET 10.

## Contents

- [Installation](#installation)
- [Core package](#core-package)
  - [Errors](#errors)
  - [Results](#results)
  - [Functional operations](#functional-operations)
  - [Asynchronous operations](#asynchronous-operations)
  - [JSON](#json)
- [ASP.NET Core package](#aspnet-core-package)
- [Fluent assertions package](#fluent-assertions-package)
- [Samples](#samples)
- [Migrating from 2.x](#migrating-from-2x)

## Installation

Install the packages you need from NuGet:

```shell
dotnet add package Kovecses.Result
dotnet add package Kovecses.Result.AspNetCore
dotnet add package Kovecses.Result.FluentAssertions
```

`Kovecses.Result.AspNetCore` and `Kovecses.Result.FluentAssertions` depend on `Kovecses.Result`. The ASP.NET Core package uses the shared `Microsoft.AspNetCore.App` framework. The assertions package depends on `xunit.assert`.

## Core package

Install [Kovecses.Result](https://www.nuget.org/packages/Kovecses.Result) for the core types and operations.

### Errors

An `Error` is a record with a non-empty `Code`, non-null `Message`, an `ErrorType`, and optional metadata. `ErrorType` identifies the technical category; its values are `Failure`, `Unexpected`, `Validation`, `Conflict`, `NotFound`, `Unauthorized`, `Forbidden`, `Timeout`, and `Canceled`. `ErrorCodes` provides a `General.*` constant for each category.

Use the built-in factories or construct an error directly:

| Factory | Resulting type | Arguments |
|---|---|---|
| `Error.Validation(code, message, metadata = null)` | `Validation` | Code, message, optional field metadata |
| `Error.NotFound(code = ErrorCodes.NotFound, message = ...)` | `NotFound` | Optional code and message |
| `Error.Unauthorized(code = ErrorCodes.Unauthorized, message = ...)` | `Unauthorized` | Optional code and message |
| `Error.Forbidden(code = ErrorCodes.Forbidden, message = ...)` | `Forbidden` | Optional code and message |
| `Error.Conflict(code = ErrorCodes.Conflict, message = ...)` | `Conflict` | Optional code and message |
| `Error.Failure(code = ErrorCodes.Failure, message = ...)` | `Failure` | Optional code and message |
| `Error.Timeout(code = ErrorCodes.Timeout, message = ...)` | `Timeout` | Optional code and message |
| `Error.Unexpected(code = ErrorCodes.Unexpected, message = ...)` | `Unexpected` | Optional code and message |
| `Error.Canceled(code = ErrorCodes.Canceled, message = ...)` | `Canceled` | Optional code and message |

For the eight factories other than `Validation`, positional arguments are ordered `(code, message)`. Use named arguments when setting only one value. This follows the constructor order `new Error(code, message, type, metadata)`.

The bookstore sample uses a domain-specific code while retaining the `NotFound` category:

```csharp
public static Error NotFound(string isbn)
    => Error.NotFound(message: $"Book '{isbn}' was not found.", code: Codes.NotFound);
```

Source: [BookErrors.cs](https://github.com/peterkovecses/Result/blob/master/samples/Kovecses.Result.Sample.BookStore.Core/Domain/Catalog/BookErrors.cs).

`Code` and `Message` are validated when created and when changed with a record `with` expression. Code must not be null, empty, or whitespace; message must not be null. Validation failures throw argument exceptions.

### Results

Use `Result.Success(...)` or `Result.Failure(...)` to create results. The non-generic `Result` represents success or failure without a value; `Result<TData>` also carries `Data` on success.

- `Result.Success(metadata = null)` creates a successful non-generic result.
- `Result.Success<TData>(data, metadata = null)` creates a successful result with data.
- Both `Result.Failure` and `Result.Failure<TData>` accept an `Error`, `(code, message)`, `(code, message, ErrorType)`, or `IEnumerable<Error>`, with optional result metadata.
- `Result.CreateFailure<TResponse>(Error[])` creates a failure for `Result` or `Result<TData>` response types.
- `Result.Combine(params Result[])` succeeds when every input succeeds; otherwise it returns the errors from all failed inputs.

Errors, error arrays, and `List<Error>` values implicitly convert to both result types. A value implicitly converts to a successful `Result<TData>`. These conversions are useful when a sample handler returns a value or errors directly; see the [bookstore samples](https://github.com/peterkovecses/Result/blob/master/samples/README.md).

`Errors` is `null` for success and an `Error[]` for failure. `Metadata` is optional. Both result types expose `IsSuccess`, `IsFailure`, `FirstError`, and `FirstErrorMessage`; `Result<TData>` also exposes `Data`. `ValueOrThrow(exceptionFactory = null)` returns data on success or throws on failure, using `InvalidOperationException` by default. `ValueOrDefault(defaultValue = default)` returns data or the supplied fallback.

The bookstore lookup handler maps a repository result into a DTO:

```csharp
public sealed record GetBookQuery(string Isbn) : IRequest<Result<BookDto>>;

public sealed class GetBookHandler(IBookRepository books) : IRequestHandler<GetBookQuery, Result<BookDto>>
{
    public async Task<Result<BookDto>> HandleAsync(GetBookQuery request, CancellationToken cancellationToken)
        => (await books.GetAsync(request.Isbn, cancellationToken)).Map(BookDto.From);
}
```

Source: [GetBook.cs](https://github.com/peterkovecses/Result/blob/master/samples/Kovecses.Result.Sample.BookStore.Core/Features/Catalog/GetBook/GetBook.cs).

### Functional operations

`Result` and `Result<TData>` provide `Match` and `MatchFirst`, plus `MatchAsync` and `MatchFirstAsync`. `Match` passes the complete error array to its failure callback; `MatchFirst` passes the first error. Their asynchronous variants take task-returning callbacks.

`Result<TData>` provides `Map` and `MapAsync` to transform successful data, `Bind` and `BindAsync` to continue with another result, and `Tap` and `TapAsync` to run a side effect without replacing the result. Non-generic `Result` provides `Bind`, `BindAsync`, `Tap`, and `TapAsync`.

The core `ResultExtensions` add:

- `Ensure` for a synchronous predicate, plus `EnsureAsync` overloads for synchronous or asynchronous predicates on `Result<TData>` and `Task<Result<TData>>`.
- `ToResult(data)` to add data to a non-generic result, or `ToResult()` to discard generic result data.
- `JoinErrorMessages(separator = "; ")` to join the messages on a non-generic result.

The bookstore batch-restock handler combines failures before saving and uses `BindAsync` to continue only when the combined result succeeds:

```csharp
var rejections = Result.Combine(restocked);

if (rejections.IsFailure)
{
    logger.LogWarning("Restock rejected: {Reasons}", rejections.JoinErrorMessages());
}

return await rejections.BindAsync(() => SaveAllAsync(request.Items, cancellationToken));
```

Source: [RestockBooks.cs](https://github.com/peterkovecses/Result/blob/master/samples/Kovecses.Result.Sample.BookStore.Core/Features/Catalog/RestockBooks/RestockBooks.cs).

### Asynchronous operations

The core `ResultExtensions` also support `Task<Result>` and `Task<Result<TData>>`:

- `BindAsync` chains a task-returning operation for `Task<Result>`; for `Task<Result<TData>>`, it accepts a callback returning either `Result<TNewData>` or `Task<Result<TNewData>>`.
- `MapAsync` on `Task<Result<TData>>` accepts a synchronous or task-returning mapping function.
- `TapAsync` on either task result accepts a synchronous action or task-returning function; generic overloads receive the data.
- `MatchAsync` on either task result accepts synchronous callbacks or task-returning callbacks.
- `EnsureAsync` accepts a synchronous or task-returning predicate for `Task<Result<TData>>`.

These operations skip success callbacks on failure and carry the failure forward.

### JSON

`Error` and both result types have `System.Text.Json` converters registered automatically. Serialize and deserialize them with `JsonSerializer`; the converters honor `JsonSerializerOptions` naming policies, including camel case. Error payloads with a missing, null, empty, or whitespace code, or a missing or null message, fail deserialization with `JsonException` rather than being replaced with placeholder values. An omitted error type defaults to `ErrorType.Failure`.

The [Minimal API sample](https://github.com/peterkovecses/Result/tree/master/samples/Kovecses.Result.Sample.MinimalApi) includes HTTP round-trip tests for wrapped results.

## ASP.NET Core package

Install [Kovecses.Result.AspNetCore](https://www.nuget.org/packages/Kovecses.Result.AspNetCore) to map results to Minimal API `IResult` or controller `IActionResult` responses. `ToMinimalApiResult` and `ToActionResult` have synchronous and `Task`-based overloads for `Result` and `Result<TData>`. `MatchToMinimalApiResult` and `MatchToActionResult` provide the same sync/async and generic/non-generic variants, with a success callback and optional failure callback receiving `Error[]`. `Error` also has direct `ToMinimalApiResult` and `ToActionResult` extensions.

By default, successful non-generic results map to `204 No Content`; successful generic results map to `200 OK` with data, or `204 No Content` when data is null. Set `includeResultInResponse: true` to return the full result object. Failures map to `ProblemDetails`; all-validation failures map to `ValidationProblemDetails`. For mixed or non-validation errors, the first error determines the status and detail. Multiple errors are included in the response extensions. An optional failure callback on a `MatchTo...` method overrides the default failure mapping.

| `ErrorType` | HTTP status |
|---|---:|
| `Failure` | 400 |
| `Unexpected` | 500 |
| `Validation` | 400 |
| `Conflict` | 409 |
| `NotFound` | 404 |
| `Unauthorized` | 401 |
| `Forbidden` | 403 |
| `Timeout` | 408 |
| `Canceled` | 400 |

The [Minimal API](https://github.com/peterkovecses/Result/tree/master/samples/Kovecses.Result.Sample.MinimalApi) and [Controllers](https://github.com/peterkovecses/Result/tree/master/samples/Kovecses.Result.Sample.Controllers) samples show bookstore endpoints, validation metadata, and custom payment-decline responses.

## Fluent assertions package

Install [Kovecses.Result.FluentAssertions](https://www.nuget.org/packages/Kovecses.Result.FluentAssertions) to use `Should()` extensions for `Result`, `Result<TData>`, and nullable `Error`.

- Result assertions: `BeSuccess`, `BeFailure`, `HaveErrors`, `HaveError(code)`, `HaveValidationErrorFor(field)`, and `HaveMetadata(key)` / `HaveMetadata(key, value)`.
- Generic result assertions add `HaveData(value)`, `HaveData(inspector)`, `HaveData(predicate)`, `WhichData`, and `ThrowOnValueAccess<TException>(exceptionFactory = null)`.
- Error assertions: `NotBeNull`, `HaveCode`, `HaveMessage`, `HaveType`, `HaveMetadata(key)` / `HaveMetadata(key, value)`, and `HaveValidationProperty(field)`.
- Error collection assertions: `HaveCount`, `AllBeOfType`, and `Contain(predicate)`.
- Validation message assertions: `Contain`, `ContainAll`, and `HaveCount`; `ValidationPropertyAssertions` also provides `BeExactly`. Chained error, collection, and validation-property assertions expose `And` when a parent assertion is available.
- `ValidationAssertions<TParent>.ExtractMessages(error, field)` retrieves validation messages for custom assertions.

Metadata assertions support metadata values in memory or after JSON deserialization. The [bookstore tests](https://github.com/peterkovecses/Result/tree/master/samples/Kovecses.Result.Sample.BookStore.Core.Tests) and [HTTP integration tests](https://github.com/peterkovecses/Result/tree/master/samples) use these assertions.

## Samples

The repository includes a shared bookstore core, its unit tests, a Minimal API host, and a Controllers host. The feature matrix and instructions are in [samples/README.md](https://github.com/peterkovecses/Result/blob/master/samples/README.md); the project source and tests provide compiling examples of the APIs described above.

## Migrating from 2.x

- **Error factory arguments:** `NotFound`, `Unauthorized`, `Forbidden`, `Conflict`, `Failure`, `Timeout`, `Unexpected`, and `Canceled` now take `(code, message)`. Change positional calls to the new order or use named arguments, such as `Error.NotFound(message: "Book not found")`.
- **Error validation:** `Error.Custom` was removed; construct `new Error(code, message, type, metadata)` or use a typed factory. Empty or whitespace codes and null messages now throw. `with` expressions validate updated `Code` and `Message` values. Invalid deserialized errors now throw `JsonException` instead of receiving fallback values.

See the [3.0.0 release notes](https://github.com/peterkovecses/Result/blob/master/RELEASE_NOTES.md) for before-and-after examples and migration details.
