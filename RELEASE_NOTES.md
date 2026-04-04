# Release Notes - v2.3.0

## 🚀 New Features

**Built-in JSON Serialization Support**

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