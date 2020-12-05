using System;
using System.IO;
using Newtonsoft.Json;

namespace Bearded.TD.Content.Serialization.Converters
{
    sealed class DiscreteSpaceTime1Converter<T> : JsonConverterBase<T>
    {
        private readonly Func<long, T> convert;

        public DiscreteSpaceTime1Converter(Func<long, T> constructor)
        {
            convert = constructor;
        }

        protected override T ReadJson(JsonReader reader, JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.Float || reader.TokenType == JsonToken.Integer)
                return convert(Convert.ToInt64(reader.Value));

            throw new InvalidDataException(
                $"Expected number value, encountered {reader.TokenType} when parsing {typeof(T).Name}.");
        }
    }
}
