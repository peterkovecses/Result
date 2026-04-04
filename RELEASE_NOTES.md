# Release Notes - v2.3.1

## 🐛 Bug Fixes

- Improved JSON deserialization for cross-option compatibility
- Fixed an issue where deserializing `Result<T>` with different `JsonSerializerOptions` than used for serialization would result in empty/default Data values.
- The library now enforces case-insensitive property matching for all nested objects during deserialization, ensuring that:
  - camelCase JSON can be deserialized with default (PascalCase) options
  - PascalCase JSON can be deserialized with Web/camelCase options
  - External API responses work regardless of the client's serializer configuration

## ⬆️ Upgrade Notes

- Non-breaking update. Fully backward compatible with v2.3.0.