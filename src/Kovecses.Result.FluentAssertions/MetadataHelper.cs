using System.Text.Json;
using Xunit;

namespace Kovecses.Result.FluentAssertions;

internal static class MetadataHelper
{
    internal static void AssertMetadataValue(object? expectedValue, object? actualValue)
    {
        if (actualValue is JsonElement element)
        {
            var value = GetValueFromElement(element);
            Assert.Equal(expectedValue?.ToString(), value?.ToString());

            return;
        }

        Assert.Equal(expectedValue, actualValue);
    }

    private static object? GetValueFromElement(JsonElement element) => element.ValueKind switch
    {
        JsonValueKind.String => element.GetString(),
        JsonValueKind.Number => element.GetDouble(),
        JsonValueKind.True => true,
        JsonValueKind.False => false,
        JsonValueKind.Null => null,
        _ => element.GetRawText()
    };
}
