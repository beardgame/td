using System;
using System.IO;
using Newtonsoft.Json;

namespace Bearded.TD.Content.Serialization.Converters;

class SpaceTime2Converter<T> : JsonConverterBase<T>
{
    private readonly Func<float, float, T> convert;

    public SpaceTime2Converter(Func<float, float, T> constructor)
    {
        convert = constructor;
    }

    protected override T ReadJson(JsonReader reader, JsonSerializer serializer)
    {
        readArrayToken(reader, JsonToken.StartArray);

        var x = readNumber(reader);
        var y = readNumber(reader);

        confirmArrayToken(reader, JsonToken.EndArray);

        return convert(x, y);
    }

    private static void readArrayToken(JsonReader reader, JsonToken arrayToken)
    {
        confirmArrayToken(reader, arrayToken);
        reader.Read();
    }

    private static void confirmArrayToken(JsonReader reader, JsonToken arrayToken)
    {
        if (reader.TokenType != arrayToken)
            throw new InvalidDataException(
                $"Expected two component array, encountered {reader.TokenType} when parsing {typeof(T).Name}.");
    }

    private static float readNumber(JsonReader reader)
    {
        if (reader.TokenType != JsonToken.Float && reader.TokenType != JsonToken.Integer)
            throw new InvalidDataException(
                $"Expected number value, encountered {reader.TokenType} when parsing {typeof(T).Name}.");

        var number = Convert.ToSingle(reader.Value);
        reader.Read();

        return number;
    }
}
