# Release v2.0.0

This is a major architectural overhaul focusing on type-safe multi-error handling, performance, and standard compliance.

## Breaking Changes
- **Result Structure:** The `Error` property has been replaced with an `Errors` collection (`IReadOnlyList<Error>`).
- **Validation Mapping:** `Error.Validation` now creates a flat error object (code and message) instead of using a metadata dictionary.
- **Fluent Assertions:** Nearly all assertion methods have been updated to work with error collections. Use `.HaveError(code)` instead of `.HaveErrorCode(code)`.

## New Features
- **Multi-Error Support:** Results can now carry multiple errors simultaneously.
- **Improved Combine:** `Result.Combine` now aggregates all errors from all input results into a single flat list.
- **Automatic RFC 7807 Support:** `Kovecses.Result.AspNetCore` now automatically maps validation-only failures to `ValidationProblemDetails`.
- **High Performance:** Introduced `FailureFactory<TResponse>` with cached delegates for near-instant generic result instantiation.
- **String Enums:** `ErrorType` now serializes to readable strings (e.g., "Validation") by default.

## Testing Enhancements
- Added `ErrorCollectionAssertions` for verifying hibalista properties (count, types, etc.).
- Deep chaining support for error properties in tests.
