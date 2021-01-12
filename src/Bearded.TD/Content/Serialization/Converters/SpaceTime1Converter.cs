using System;
using System.Text.Json;

namespace Bearded.TD.Content.Serialization.Converters
{
    sealed class SpaceTime1Converter<T> : JsonConverterBase<T>
    {
        private readonly Func<float, T> convert;

        public SpaceTime1Converter(Func<float, T> constructor)
        {
            convert = constructor;
        }

        protected override T ReadJson(ref Utf8JsonReader reader, JsonSerializerOptions options)
        {
            if (!reader.TryGetSingle(out var raw))
            {
                throw new JsonException(
                    $"Expected number value, encountered {reader.TokenType} when parsing {typeof(T).Name}.");
            }

            return convert(raw);
        }
    }
}
