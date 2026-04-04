# Release Notes - Kovecses.Result 2.5.0

## New Features
- **Implicit Error Collection Conversions:** Added implicit operators for `Error[]` and `List<Error>` to both `Result` and `Result<T>`. This enables cleaner code when aggregating multiple domain or validation errors.
- **Collection Expression Support:** Full support for C# 12 collection expressions (e.g., `return [Error.Failure("...")]`).
- **Boilerplate Reduction:** Handlers can now return error lists or arrays directly, which are automatically converted to the appropriate failure result type.

## Improvements
- Improved documentation and samples showcasing realistic error aggregation patterns.
