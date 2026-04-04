# Release v2.2.0

This release introduces a "Pure POCO" architecture to ensure seamless JSON serialization and deserialization across all .NET environments, while maintaining the collection-based error handling.

## Improvements & Fixes
- **Pure POCO Architecture:** Transitioned `Result` and `Result<T>` to use parameterless constructors with `[JsonConstructor]` and `init` properties.
- **Factory Method Refinement:** Centralized instantiation through `internal static Create` methods, shielding the object initialization logic from public API while remaining fully compatible with serializable property patterns.
- **Enhanced Compatibility:** Guaranteed out-of-the-box support for `System.Text.Json` and `Newtonsoft.Json` without requiring custom configuration or converters.
- **Stability:** Maintained the multi-error `Errors` collection and immutability as core pillars.
