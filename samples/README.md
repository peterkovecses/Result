# Kovecses.Result samples

Two realistic applications that together demonstrate every feature of the Result library (`Kovecses.Result`, `Kovecses.Result.AspNetCore`, `Kovecses.Result.FluentAssertions`), both built on one shared bookstore core:

| Project | Description | Status |
|---|---|---|
| `Kovecses.Result.Sample.BookStore.Core` | Shared bookstore domain: catalog and orders, vertical slices, in-memory persistence | done |
| `Kovecses.Result.Sample.BookStore.Core.Tests` | Unit tests for every handler, validator and the domain | done |
| MinimalApi host | Minimal API on top of the Core, handlers injected directly | done |
| `Kovecses.Result.Sample.MinimalApi.Tests` | Minimal API integration tests, including HTTP status mapping and wrapped-result JSON round trips | done |
| Controllers host | Thin controllers on top of the Core, via `IMediator` | planned |

> The older Employee Core, Controllers and Tests projects still exist and will be removed after both new hosts are in place.

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
| `Result.Success()` / `Success(data)` / `Success(metadata)` | `ValidationErrors`, `RestockBooks`, `CancelOrder`, `SearchBooks`, `GetOrderSummary` | Returned by the Core handlers to the catalog and orders endpoints | planned |
| `Result.Failure(...)` overloads (`Error`, code + message, code + message + type, `IEnumerable<Error>`, generic variants) | `Order.Place`, `FakePaymentGateway`, `ValidationErrors`, `GetManyAsync`, `PlaceOrder` | `PlaceOrderEndpoint` (passes non-payment failures through to the standard mapper) | planned |
| Implicit conversions from `Error`, `Error[]`, `List<Error>` and data | `InMemoryBookRepository`, `AddBook`, `PlaceOrder`, `CurrentUser`, `CancelOrder`, `Order.Place` | — (Core concern) | planned |
| `Result.CreateFailure<T>` | `ValidationDecorator` (`Application/Abstractions.cs`) | — (Core concern) | planned |
| `Result.Combine` | `GetManyAsync`, `RestockBooks`, `ValidationDecorator` | — (Core concern) | planned |
| `Match`, `MatchFirst` | `GetManyAsync`, `PlaceOrder`, `CancelOrder` | — (Core concern) | planned |
| `MatchAsync`, `MatchFirstAsync` | `GetOrderSummary`, `CancelOrder` | — (Core concern) | planned |
| `Map`, `Bind`, `Tap`, `Ensure` (sync) | `Book`, `Order`, `PlaceOrder`, `GetBook`, `GetOrderSummary` | — (Core concern) | planned |
| `MapAsync`, `BindAsync`, `TapAsync`, `EnsureAsync` on `Task<Result<T>>` | `AddBook`, `ChangeBookPrice`, `RetireBook`, `GetOrder`, `PlaceOrder` | — (Core concern) | planned |
| `BindAsync` on `Result` and `Result<T>` instances | `RestockBooks`, `GetOrder`, `PlaceOrder` | — (Core concern) | planned |
| `EnsureAsync` with an async predicate | `RetireBook` | — (Core concern) | planned |
| `ValueOrThrow` | `BookSeedData`, `GetManyAsync`, `PlaceOrder`, `RestockBooks` | — (Core concern) | planned |
| `ValueOrDefault` | `GetOrderSummary` | — (Core concern) | planned |
| `ToResult` (`Result<T>` to `Result`) | `RetireBook`, `RestockBooks` | — (Core concern) | planned |
| `JoinErrorMessages` | `PlaceOrder`, `RestockBooks` | — (Core concern) | planned |
| `FirstErrorMessage` | `PlaceOrder` | — (Core concern) | planned |
| Success and error metadata | `RestockBooks`, `CancelOrder` (success); `BookErrors.InsufficientStock`, `ValidationErrors` (error) | Validation metadata maps to `ValidationProblemDetails` on catalog endpoints | planned |
| `Error` `with { Metadata = ... }` | `BookErrors.InsufficientStock` | — (Core concern) | planned |
| Error factories (`NotFound`, `Unauthorized`, `Forbidden`, `Conflict`, `Failure`, `Timeout`, `Unexpected`, `Canceled`, `Validation`) | `BookErrors`, `OrderErrors`, `CurrentUser`, `ValidationErrors` | All ErrorTypes map through `ToMinimalApiResult`; payment declines map to 402 in `PlaceOrderEndpoint` | planned |
| `Kovecses.Result.FluentAssertions` | `BookStore.Core.Tests` (`BeSuccess`, `BeFailure`, `HaveError`, `HaveType`, `HaveData`, `HaveMetadata`, `HaveErrors`, `HaveValidationProperty`, `ThrowOnValueAccess`) | `Sample.MinimalApi.Tests` (`BeSuccess`, `BeFailure`, `HaveType`) | planned |
| `Kovecses.Result.AspNetCore` (HTTP result mapping, ProblemDetails) | n/a (host concern) | Catalog/order endpoints (`ToMinimalApiResult`, async and Match variants, direct Error mapping, wrapped internal response) | planned |
| JSON serialization of results and errors | n/a (host concern) | Internal book endpoint and wrapped-result integration tests | planned |
