# Kovecses.Result samples

Two realistic applications that together demonstrate every feature of the Result library (`Kovecses.Result`, `Kovecses.Result.AspNetCore`, `Kovecses.Result.FluentAssertions`), both built on one shared bookstore core:

| Project | Description | Status |
|---|---|---|
| `Kovecses.Result.Sample.BookStore.Core` | Shared bookstore domain: catalog and orders, vertical slices, in-memory persistence | done |
| `Kovecses.Result.Sample.BookStore.Core.Tests` | Unit tests for every handler, validator and the domain | done |
| MinimalApi host | Minimal API on top of the Core, handlers injected directly | planned |
| Controllers host | Thin controllers on top of the Core, via `IMediator` | planned |

> The older `Kovecses.Result.Sample.Core`, `.MinimalApi`, `.Controllers` and `.Tests` projects (Employee domain) still exist and will be removed once the new hosts are in place.

## BookStore Core

```
Application/     IRequestHandler, IValidator, IMediator, validation decorator, ICurrentUser, IPaymentGateway
Domain/          Catalog (Book, BookErrors) and Orders (Order, OrderErrors) plus repository abstractions
Infrastructure/  In-memory repositories, fake payment gateway, seed data
Features/        Catalog/{GetBook, SearchBooks, AddBook, ChangeBookPrice, RestockBooks, RetireBook}
                 Orders/{PlaceOrder, GetOrder, CancelOrder, GetOrderSummary}
```

- Every use case lives in `Features/<Area>/<UseCase>/` with its request, handler and (where needed) validator.
- `services.AddBookStoreCore()` registers every handler wrapped in a validation decorator. A request without a validator (for example `GetBook`) simply passes through.
- Handlers can be injected directly (`IRequestHandler<TRequest, TResult>`) or dispatched with `IMediator.SendAsync<TRequest, TResult>(...)`.
- The host provides `ICurrentUser` and logging.

### Error types in natural situations

| `ErrorType` | Where |
|---|---|
| `NotFound` | Unknown book or order |
| `Validation` | Field-level metadata from the validators (single `General.Validation` error, metadata `field -> messages[]`) |
| `Conflict` | Duplicate ISBN, retired book, insufficient stock, order already cancelled |
| `Unauthorized` | `PlaceOrder`, `GetOrder`, `CancelOrder`, `GetOrderSummary` without a current user |
| `Forbidden` | Reading or cancelling another customer's order |
| `Failure` | Domain codes such as `Catalog.BookHasOpenOrders`, `Orders.PaymentDeclined`, `Orders.EmptyOrder` |
| `Timeout`, `Canceled`, `Unexpected` | Payment gateway problems in `PlaceOrder` |

## Feature coverage matrix

Core points to the file in `BookStore.Core` that shows the feature. The host columns are filled in by the host migration tickets.

| Library feature | Core | MinimalApi | Controllers |
|---|---|---|---|
| `Result.Success()` / `Success(data)` / `Success(metadata)` | `RestockBooks`, `CancelOrder`, `SearchBooks`, `GetOrderSummary` | planned | planned |
| `Result.Failure(...)` overloads (`Error`, code + message, code + message + type, `IEnumerable<Error>`, generic variants) | `Order.Place`, `FakePaymentGateway`, `ValidationErrors`, `GetManyAsync`, `PlaceOrder` | planned | planned |
| Implicit conversions from `Error`, `Error[]`, `List<Error>` and data | `InMemoryBookRepository`, `AddBook`, `PlaceOrder`, `CurrentUser`, `CancelOrder`, `Order.Place` | planned | planned |
| `Result.CreateFailure<T>` | `ValidationDecorator` (`Application/Abstractions.cs`) | planned | planned |
| `Result.Combine` | `GetManyAsync`, `PlaceOrder`, `RestockBooks`, `ValidationDecorator` | planned | planned |
| `Match`, `MatchFirst` | `GetManyAsync`, `PlaceOrder`, `CancelOrder` | planned | planned |
| `MatchAsync`, `MatchFirstAsync` | `GetOrderSummary`, `CancelOrder` | planned | planned |
| `Map`, `Bind`, `Tap`, `Ensure` (sync) | `Book`, `Order`, `PlaceOrder`, `GetBook`, `GetOrderSummary` | planned | planned |
| `MapAsync`, `BindAsync`, `TapAsync`, `EnsureAsync` on `Task<Result<T>>` | `AddBook`, `ChangeBookPrice`, `RetireBook`, `GetOrder`, `PlaceOrder` | planned | planned |
| `BindAsync` on `Result` and `Result<T>` instances | `RestockBooks`, `GetOrder`, `PlaceOrder` | planned | planned |
| `EnsureAsync` with an async predicate | `RetireBook` | planned | planned |
| `ValueOrThrow` | `BookSeedData`, `GetManyAsync`, `PlaceOrder`, `RestockBooks` | planned | planned |
| `ValueOrDefault` | `GetOrderSummary` | planned | planned |
| `ToResult` / `ToResult(data)` | `RetireBook`, `RestockBooks` | planned | planned |
| `JoinErrorMessages` | `PlaceOrder`, `RestockBooks` | planned | planned |
| `FirstErrorMessage` | `PlaceOrder` | planned | planned |
| Success and error metadata | `RestockBooks`, `CancelOrder` (success); `BookErrors.InsufficientStock`, `ValidationErrors` (error) | planned | planned |
| `Error` `with { Metadata = ... }` | `BookErrors.InsufficientStock` | planned | planned |
| Error factories (`NotFound`, `Unauthorized`, `Forbidden`, `Conflict`, `Failure`, `Timeout`, `Unexpected`, `Canceled`, `Validation`) | `BookErrors`, `OrderErrors`, `CurrentUser`, `ValidationErrors` | planned | planned |
| `Kovecses.Result.FluentAssertions` | `BookStore.Core.Tests` (`BeSuccess`, `BeFailure`, `HaveError`, `HaveType`, `HaveData`, `HaveMetadata`, `HaveErrors`, `HaveValidationProperty`, `ThrowOnValueAccess`) | planned | planned |
| `Kovecses.Result.AspNetCore` (HTTP result mapping, ProblemDetails) | n/a (host concern) | planned | planned |
| JSON serialization of results and errors | n/a (host concern) | planned | planned |
