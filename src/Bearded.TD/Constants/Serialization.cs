using System.Text.Json;
using System.Text.Json.Serialization;

namespace Bearded.TD
{
    static partial class Constants
    {
        public static class Serialization
        {
            public static readonly JsonSerializerOptions DefaultJsonSerializerOptions =
                new()
                {
                    AllowTrailingCommas = true,
                    IgnoreNullValues = true,
                    IncludeFields = true,
                    PropertyNameCaseInsensitive = true,
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                    WriteIndented = true
                };

            public static readonly JsonStringEnumConverter StringEnumConverter = new(JsonNamingPolicy.CamelCase);
        }
    }
}
