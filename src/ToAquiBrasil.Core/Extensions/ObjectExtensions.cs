using System.Text.Json;

namespace ToAquiBrasil.Core.Extensions;

public static class ObjectExtensions
{
    public static string SerializeWithCamelCase(this object value)
    {
        return JsonSerializer.Serialize(value,
            new JsonSerializerOptions {PropertyNamingPolicy = JsonNamingPolicy.CamelCase});
    }
}