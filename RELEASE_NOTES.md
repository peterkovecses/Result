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

**Version Bump:** Patch release (2.4.0 → 2.4.1)  
**Breaking Changes:** None
