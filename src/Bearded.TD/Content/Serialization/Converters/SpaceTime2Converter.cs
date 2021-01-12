using System;
using System.Text.Json;

namespace Bearded.TD.Content.Serialization.Converters
{
    sealed class SpaceTime2Converter<T> : JsonConverterBase<T>
    {
        private readonly Func<float, float, T> convert;

        public SpaceTime2Converter(Func<float, float, T> constructor)
        {
            convert = constructor;
        }

        protected override T ReadJson(ref Utf8JsonReader reader, JsonSerializerOptions options)
        {
            readArrayToken(ref reader, JsonTokenType.StartArray);

            var x = readNumber(ref reader);
            var y = readNumber(ref reader);

            confirmArrayToken(ref reader, JsonTokenType.EndArray);

            return convert(x, y);
        }

        private static void readArrayToken(ref Utf8JsonReader reader, JsonTokenType arrayToken)
        {
            confirmArrayToken(ref reader, arrayToken);
            reader.Read();
        }

        private static void confirmArrayToken(ref Utf8JsonReader reader, JsonTokenType arrayToken)
        {
            if (reader.TokenType != arrayToken)
            {
                throw new JsonException(
                    $"Expected two component array, encountered {reader.TokenType} when parsing {typeof(T).Name}.");
            }
        }

        private static float readNumber(ref Utf8JsonReader reader)
        {
            if (!reader.TryGetSingle(out var number))
            {
                throw new JsonException(
                    $"Expected number value, encountered {reader.TokenType} when parsing {typeof(T).Name}.");
            }

            reader.Read();

            return number;
        }
    }
}
