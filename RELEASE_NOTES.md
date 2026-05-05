# Release Notes - Kovecses.Result 2.10.0

## New Features
- **Enhanced Fluent Assertions (Kovecses.Result.FluentAssertions):**
    - **`HaveData(Action<TData> inspector)`:** Added a new overload that enables deep inspection of the result's data using a lambda. It automatically asserts that the result is successful and the data is not null before executing the inspector.
    - **`WhichData` property:** Added a new property to `ResultAssertions<TData>` that allows direct access to the data for further chaining. It automatically performs success and null-check assertions.

---

# Release Notes - Kovecses.Result 2.9.0

## New Features
- **Async Result Mapping Extensions:** Added `ToActionResultAsync` and `ToMinimalApiResultAsync` for `Task<Result>` and `Task<Result<TData>>`. This enables a clean, fluent syntax for asynchronous controller actions and Minimal API endpoints (e.g., `return await mediator.Send(query).ToActionResultAsync();`).
- **Smart Mapping Overloads:** Introduced `MatchToActionResult` and `MatchToMinimalApiResult` (with async counterparts). These methods allow you to handle the success case (e.g., returning `201 Created`) while the library automatically handles the failure mapping to `ProblemDetails`.
- **Custom Failure Mapping:** The smart mapping methods also support an optional `onFailure` lambda for highly specific error handling scenarios.

---

# Release Notes - Kovecses.Result 2.8.0

## ⚠️ Breaking Change: Match Overload Renamed to MatchFirst
To resolve type inference ambiguity the `Match` overload that accepts a single `Error` has been renamed to **`MatchFirst`** (and `MatchFirstAsync`).

---

# Release Notes - Kovecses.Result 2.7.0

## New Features
- **Added `FirstError` and `FirstErrorMessage` properties:** Provides easy, direct access to the most common error case, significantly improving code readability in clients.
- **Added `Match` overloads for single errors:** You can now use `result.Match(onSuccess, (Error err) => ...)` instead of dealing with an error array when only the primary failure matters.
- **Added `JoinErrorMessages` extension:** New helper to easily aggregate all error messages into a single string with a custom separator (e.g., `result.JoinErrorMessages(" | ")`).
- **Added `Failure` overloads with `ErrorType`:** New methods in `Result` and `Result<T>` that allow specifying the technical error type (e.g. `NotFound`, `Conflict`) alongside a custom code and message, ensuring correct HTTP mapping when wrapping errors.

---

# Release Notes - Kovecses.Result 2.6.0

## New Features
- **Enabled Implicit Conversion for Bubbling Errors:** Changed the `Errors` property type from `IReadOnlyList<Error>` to `Error[]`. This enables implicit conversion when returning `result.Errors` from a method that returns a `Result` or `Result<T>`, significantly reducing boilerplate when bubbling up failures.
- **Improved Collection Expression Support:** Full native support for C# 12 collection expressions (`[]`) via the `Error[]` concrete type.

## Breaking Changes
- **Property Type Change:** `Result.Errors` is now `Error[]?` instead of `IReadOnlyList<Error>?`. Since `Error[]` implements `IReadOnlyList<Error>`, this is source-compatible for most read operations, but might require updates if you specifically relied on the interface type in your own method signatures or if you used `.Count` instead of `.Length` (though `.Count()` extension method still works).

---

# Release Notes - Kovecses.Result 2.5.0

## New Features
- **Implicit Error Collection Conversions:** Added implicit operators for `Error[]` and `List<Error>` to both `Result` and `Result<T>`. This enables cleaner code when aggregating multiple domain or validation errors.
- **Collection Expression Support:** Full support for C# 12 collection expressions (e.g., `return [Error.Failure("...")]`).
- **Boilerplate Reduction:** Handlers can now return error lists or arrays directly, which are automatically converted to the appropriate failure result type.

## Improvements
- Improved documentation and samples showcasing realistic error aggregation patterns.

---

# Release Notes - v2.4.1

## 🐛 Bug Fix: JSON Deserialization Support for Validation Metadata
When validation errors with metadata were serialized to JSON and deserialized, the `HaveValidationProperty()` FluentAssertions method failed because metadata values became `List<object>` instead of `List<string>`.

**Fixed:** Added support for `List<object>` in `HaveValidationProperty()` to handle JSON deserialization scenarios. Now works seamlessly in integration tests and HTTP round-trips.

**Example:**
```csharp
var error = Error.Validation(
    "General.Validation",
    "Validation failed",
    new() { ["Email"] = new[] { "Email is required" } }
);

var json = JsonSerializer.Serialize(error);
var deserialized = JsonSerializer.Deserialize<Error>(json);

// ✅ Now works - handles List<object> from deserialization
deserialized.Should()
    .HaveValidationProperty("Email")
    .Contain("Email is required");
```

- **Version Bump:** Patch release (2.4.0 → 2.4.1)
- **Breaking Changes:** None

---

# Release Notes - v2.4.0

## ✨ New Features
### Enhanced Validation Error Handling with Metadata Support
The `Error.Validation()` factory method now supports an optional metadata parameter, enabling cleaner and more organized handling of validation errors in applications.

**What Changed:**
- Added optional metadata parameter to `Error.Validation(string code, string message, Dictionary<string, object>? metadata = null)`
- Recommended pattern for aggregating multiple field-level validation failures into a single error object with organized metadata
- Metadata keys are property names, values are string arrays of validation messages

**Use Case - MediatR ValidationBehavior:**
```csharp
// Aggregate validation errors by field
var validationMetadata = new Dictionary<string, object>
{
    ["Email"] = new[] { "Email is required.", "Email format is invalid." },
    ["Password"] = new[] { "Password must be at least 8 characters." }
};

var error = Error.Validation(
    ErrorCodes.Validation,
    "Validation failed.",
    validationMetadata
);

return Result.Failure(error);
```

**Resulting API Response:**
```json
{
  "data": null,
  "errors": [
    {
      "code": "General.Validation",
      "message": "Validation failed.",
      "type": 2,
      "metadata": {
        "Email": ["Email is required.", "Email format is invalid."],
        "Password": ["Password must be at least 8 characters."]
      }
    }
  ]
}
```

**Benefits:**
- ✅ Consistent error codes: All validation errors now use `ErrorCodes.Validation`
- ✅ Cleaner API responses: Multiple validation failures aggregated into a single error with organized field-level details
- ✅ Better client handling: Clients can easily parse field-level errors directly from metadata keys
- ✅ Type-safe pattern: Metadata structure is self-documenting
- ✅ Backward compatible: The parameter is optional; existing code continues to work unchanged

### Chainable Validation Property Assertions (FluentAssertions)
The `HaveValidationProperty()` method now returns a chainable `ValidationPropertyAssertions` object with an `.And` property, enabling fluent chained assertions on multiple validation properties without duplicated `.Should()` calls.

**What Changed:**
- `ValidationPropertyAssertions` now supports parent chaining via `.And` property
- When used within `ErrorAssertions<TParent>` context, `.And` returns to error assertions for multiple property validation

**Example - Fluent Chaining:**
```csharp
[Fact]
public async Task CreateUser_WithValidationFailures_ShouldAggregateErrors()
{
    var result = await _validator.ValidateAsync(new CreateUserCommand 
    { 
        FullName = "J",        // Too short
        Position = ""          // Empty
    });
    
    // Chain multiple property assertions with fluent syntax
    result.Should().BeFailure()
        .HaveError(ErrorCodes.Validation)
        .HaveValidationProperty("FullName")
            .Contain("too short")
            .And
        .HaveValidationProperty("Position")
            .Contain("required");
}
```

## ⬆️ Upgrade Notes
- Non-breaking update. Fully backward compatible with v2.3.x
- The `Error.Validation()` method signature now includes an optional third parameter
- FluentAssertions `.And` property on `ValidationPropertyAssertions` enables chaining (only available when used with `ErrorAssertions<TParent>`)
- Old code using `.with { Metadata = ... }` will continue to work
- Consider adopting the aggregated validation pattern in new validation pipelines for cleaner API responses

---

# Release Notes - v2.3.1

## 🐛 Bug Fixes
- **Improved JSON deserialization for cross-option compatibility**
- Fixed an issue where deserializing `Result<T>` with different `JsonSerializerOptions` than used for serialization would result in empty/default `Data` values.
- The library now enforces case-insensitive property matching for all nested objects during deserialization, ensuring that:
  - camelCase JSON can be deserialized with default (PascalCase) options
  - PascalCase JSON can be deserialized with Web/camelCase options
  - External API responses work regardless of the client's serializer configuration

## ⬆️ Upgrade Notes
- Non-breaking update. Fully backward compatible with v2.3.0.

---

# Release Notes - v2.3.0

## 🚀 New Features
- **Built-in JSON Serialization Support**
- Added custom `System.Text.Json` converters for `Result`, `Result<T>`, and `Error` types:
  - Full support for direct API response deserialization
  - Case-insensitive property matching
  - Proper `Dictionary<string, object>` metadata handling
  - Compatible with `JsonNamingPolicy.CamelCase` and other naming policies

## 🐛 Bug Fixes
- Fixed deserialization failure when API responses contained the `Result` object directly
- Fixed `[JsonConstructor]` accessibility for `System.Text.Json`
- Fixed metadata deserialization – values are now correctly typed instead of remaining as `JsonElement`

## ⬆️ Upgrade Notes
- Non-breaking update. The new JSON converters are automatically applied via `[JsonConverter]` attributes.

---

# Release v2.2.0
This release introduces a "Pure POCO" architecture to ensure seamless JSON serialization and deserialization across all .NET environments, while maintaining the collection-based error handling.

## Improvements & Fixes
- **Pure POCO Architecture:** Transitioned `Result` and `Result<T>` to use parameterless constructors with `[JsonConstructor]` and init properties.
- **Factory Method Refinement:** Centralized instantiation through internal static `Create` methods, shielding the object initialization logic from public API while remaining fully compatible with serializable property patterns.
- **Enhanced Compatibility:** Guaranteed out-of-the-box support for `System.Text.Json` and `Newtonsoft.Json` without requiring custom configuration or converters.
- **Stability:** Maintained the multi-error `Errors` collection and immutability as core pillars.

---

# Release v2.1.0
This release restores the POCO-friendly architecture from v1.x while maintaining the intentional multi-error collection support introduced in v2.0.0.

## Improvements & Fixes
- **POCO Compatibility:** Restored traditional class structure and protected parameterless constructors for `Result` and `Result<T>`.
- **Serialization Neutrality:** Removed forced `JsonStringEnumConverter` from `ErrorType` to follow framework-level JSON defaults.
- **Improved Deserialization:** Fixed an issue where `Result<T>` could not be deserialized correctly using `System.Text.Json` due to primary constructor usage.
- **Documentation:** Updated README with accurate JSON examples reflecting serialized output.
- **Sample Projects:** Removed manual JSON configurations from Sample and Test projects to remain neutral.

---

# Release v2.0.0
This is a major architectural overhaul focusing on type-safe multi-error handling, performance, and standard compliance.

## Breaking Changes
- **Result Structure:** The `Error` property has been replaced with an `Errors` collection (`IReadOnlyList<Error>`).
- **Validation Mapping:** `Error.Validation` now creates a flat error object (code and message) instead of using a metadata dictionary.
- **Fluent Assertions:** Nearly all assertion methods have been updated to work with error collections. Use `.HaveError(code)` instead of `.HaveErrorCode(code)`.

## New Features
- **Multi-Error Support:** Results can carry multiple errors simultaneously.
- **Improved Combine:** `Result.Combine` now aggregates all errors from all input results into a single flat list.
- **Automatic RFC 7807 Support:** `Kovecses.Result.AspNetCore` now automatically maps validation-only failures to `ValidationProblemDetails`.
- **High Performance:** Introduced `FailureFactory<TResponse>` with cached delegates for near-instant generic result instantiation.
- **String Enums:** `ErrorType` now serializes to readable strings (e.g., "Validation") by default.

## Testing Enhancements
- Added `ErrorCollectionAssertions` for verifying error list properties (count, types, etc.).
- Deep chaining support for error properties in tests.

---

# Release Notes - v1.2.1
This release focuses on improving the developer experience for testing and expanding the capabilities of the validation logic.

## 🚀 New Features & Enhancements
- **Fluent Assertion Chaining:** You can now chain multiple assertions on a single `Result` object using the new `.And` property. This makes unit tests more readable and concise.
- **Enhanced Error Assertions:** Improved the `HaveError()` assertion to support chaining, allowing you to verify the error code, message, and type in a single statement.
- **Improved Validation Support:** Refactored the internal handling of validation errors to better support multiple error messages per field.
- **New Error Type:** Added the `Canceled` error type to represent operations that were aborted.

## 🛠 Refactorings
- Updated the sample project validators to demonstrate multi-error field validation.
- Unified versioning across all packages (`Kovecses.Result`, `Kovecses.Result.AspNetCore`, and `Kovecses.Result.FluentAssertions`) to 1.2.1.

## 📦 NuGet Packages
The following packages have been updated to 1.2.1:
- `Kovecses.Result`
- `Kovecses.Result.AspNetCore`
- `Kovecses.Result.FluentAssertions`
