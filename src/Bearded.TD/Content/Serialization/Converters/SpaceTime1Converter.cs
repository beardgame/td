using System;
using System.IO;
using Newtonsoft.Json;

namespace Bearded.TD.Content.Serialization.Converters
{
    sealed class SpaceTime1Converter<T> : JsonConverterBase<T>
    {
        private readonly Func<float, T> convert;

        public SpaceTime1Converter(Func<float, T> constructor)
        {
            convert = constructor;
        }

        protected override T ReadJson(JsonReader reader, JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.Float || reader.TokenType == JsonToken.Integer)
                return convert(Convert.ToSingle(reader.Value));

            throw new InvalidDataException($"Expected number value, encountered {reader.TokenType} when parsing {typeof(T).Name}.");
        }
    }
}
