# Release Notes - Kovecses.Result 2.6.0

## New Features
- **Enabled Implicit Conversion for Bubbling Errors:** Changed the `Errors` property type from `IReadOnlyList<Error>` to `Error[]`. This enables implicit conversion when returning `result.Errors` from a method that returns a `Result` or `Result<T>`, significantly reducing boilerplate when bubbling up failures.
- **Improved Collection Expression Support:** Full native support for C# 12 collection expressions (`[]`) via the `Error[]` concrete type.

## Breaking Changes
- **Property Type Change:** `Result.Errors` is now `Error[]?` instead of `IReadOnlyList<Error>?`. Since `Error[]` implements `IReadOnlyList<Error>`, this is source-compatible for most read operations, but might require updates if you specifically relied on the interface type in your own method signatures or if you used `.Count` instead of `.Length` (though `.Count()` extension method still works).
