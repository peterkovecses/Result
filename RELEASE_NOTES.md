# Release v2.1.0

This release restores the POCO-friendly architecture from v1.x while maintaining the intentional multi-error collection support introduced in v2.0.0.

## Improvements & Fixes
- **POCO Compatibility:** Restored traditional class structure and protected parameterless constructors for `Result` and `Result<T>`.
- **Serialization Neutrality:** Removed forced `JsonStringEnumConverter` from `ErrorType` to follow framework-level JSON defaults.
- **Improved Deserialization:** Fixed an issue where `Result<T>` could not be deserialized correctly using `System.Text.Json` due to primary constructor usage.
- **Documentation:** Updated README with accurate JSON examples reflecting serialized output.
- **Sample Projects:** Removed manual JSON configurations from Sample and Test projects to remain neutral.
