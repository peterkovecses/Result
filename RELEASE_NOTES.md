# Release Notes - v2.4.0

## ✨ New Features

### Enhanced Validation Error Handling with Metadata Support

The `Error.Validation()` factory method now supports an optional metadata parameter, enabling cleaner and more organized handling of validation errors in applications.

**What Changed:**
- Added optional `metadata` parameter to `Error.Validation(string code, string message, Dictionary<string, object>? metadata = null)`
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

**ValidationPropertyAssertions Methods:**
- `.Contain(string message)` - Assert message contains text (case-insensitive)
- `.ContainAll(params string[] messages)` - Assert all messages are present
- `.HaveCount(int expected)` - Assert exact message count
- `.BeExactly(params string[] messages)` - Assert exact message match
- `.And` - Chain back to parent error assertions

## ⬆️ Upgrade Notes

- Non-breaking update. Fully backward compatible with v2.3.x
- The `Error.Validation()` method signature now includes an optional third parameter
- FluentAssertions `.And` property on `ValidationPropertyAssertions` enables chaining (only available when used with `ErrorAssertions<TParent>`)
- Old code using `.with { Metadata = ... }` will continue to work
- Consider adopting the aggregated validation pattern in new validation pipelines for cleaner API responses
