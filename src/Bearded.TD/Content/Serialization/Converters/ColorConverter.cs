using System;
using System.IO;
using Bearded.Graphics;
using Bearded.Utilities;
using Newtonsoft.Json;
using static Bearded.Utilities.Maybe;

namespace Bearded.TD.Content.Serialization.Converters;

sealed class ColorConverter : JsonConverterBase<Color>
{
    protected override Color ReadJson(JsonReader reader, JsonSerializer serializer)
    {
        if (reader.TokenType == JsonToken.String)
        {
            var s = (string)reader.Value;

            foreach (var field in typeof(Color).GetFields())
            {
                if (field.IsStatic && field.FieldType == typeof(Color)
                    && field.Name.Equals(s, StringComparison.InvariantCultureIgnoreCase))
                    return (Color)field.GetValue(null);
            }

            try
            {
                return new Color(Convert.ToUInt32(s, 16));
            }
            catch(Exception)
            {
                throw new InvalidDataException("Color has unknown or invalid string value.");
            }
        }

        if (reader.TokenType == JsonToken.StartArray)
        {
            reader.Read();

            var r = readByte(reader);
            var g = readByte(reader);
            var b = readByte(reader);
            var a = tryReadByte(reader);

            if (reader.TokenType == JsonToken.EndArray)
            {
                return new Color(r, g, b, a.ValueOrDefault(255));
            }
        }

        throw new InvalidDataException("Colour has no or invalid value.");
    }

    private static byte readByte(JsonReader reader)
    {
        return tryReadByte(reader)
            .Match(
                b => b,
                () => throw new InvalidDataException(
                    $"Expected number value, encountered {reader.TokenType} when parsing Color component (expecting integer).")
            );
    }

    private static Maybe<byte> tryReadByte(JsonReader reader)
    {
        if (reader.TokenType != JsonToken.Integer)
            return Nothing;

        var b = Convert.ToByte(reader.Value);
        reader.Read();

        return Just(b);
    }
}