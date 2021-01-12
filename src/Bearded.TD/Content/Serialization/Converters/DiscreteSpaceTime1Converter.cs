using System;
using System.Text.Json;

namespace Bearded.TD.Content.Serialization.Converters
{
    sealed class DiscreteSpaceTime1Converter<T> : JsonConverterBase<T>
    {
        private readonly Func<long, T> convert;

        public DiscreteSpaceTime1Converter(Func<long, T> constructor)
        {
            convert = constructor;
        }

        protected override T ReadJson(ref Utf8JsonReader reader, JsonSerializerOptions options)
        {
            if (!reader.TryGetInt64(out var number))
            {
                throw new JsonException(
                    $"Expected number value, encountered {reader.TokenType} when parsing {typeof(T).Name}.");
            }

            return convert(number);
        }
    }
}
