# Release Notes - Kovecses.Result 2.7.0

## New Features
- **Added `FirstError` and `FirstErrorMessage` properties:** Provides easy, direct access to the most common error case, significantly improving code readability in clients.
- **Added `Match` overloads for single errors:** You can now use `result.Match(onSuccess, (Error err) => ...)` instead of dealing with an error array when only the primary failure matters.
- **Added `JoinErrorMessages` extension:** New helper to easily aggregate all error messages into a single string with a custom separator (e.g., `result.JoinErrorMessages(" | ")`).
- **Added `Failure` overloads with `ErrorType`:** New methods in `Result` and `Result<T>` that allow specifying the technical error type (e.g. `NotFound`, `Conflict`) alongside a custom code and message, ensuring correct HTTP mapping when wrapping errors.
