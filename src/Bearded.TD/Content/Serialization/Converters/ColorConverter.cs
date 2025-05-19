using System;
using System.IO;
using Bearded.Graphics;
using Newtonsoft.Json;

namespace Bearded.TD.Content.Serialization.Converters;

sealed class ColorConverter : JsonConverterBase<Color>
{
    protected override Color ReadJson(JsonReader reader, JsonSerializer serializer)
    {
        switch (reader.TokenType)
        {
            case JsonToken.String:
                var s = (string)reader.Value!;

                foreach (var field in typeof(Color).GetFields())
                {
                    if (field.IsStatic && field.FieldType == typeof(Color)
                        && field.Name.Equals(s, StringComparison.InvariantCultureIgnoreCase))
                        return (Color)field.GetValue(null)!;
                }

                try
                {
                    return new Color(Convert.ToUInt32(s, 16));
                }
                catch(Exception)
                {
                    throw new InvalidDataException("Color has unknown or invalid string value.");
                }
            case JsonToken.StartArray:
                reader.Read();

                var r = readByte(reader);
                var g = readByte(reader);
                var b = readByte(reader);
                var a = tryReadByte(reader);

                if (reader.TokenType == JsonToken.EndArray)
                {
                    return new Color(r, g, b, a ?? 255);
                }

                break;
        }

        throw new InvalidDataException("Colour has no or invalid value.");
    }

    private static byte readByte(JsonReader reader)
    {
        return tryReadByte(reader) ?? throw new InvalidDataException(
            $"Expected number value, encountered {reader.TokenType} when parsing Color component (expecting integer).");
    }

    private static byte? tryReadByte(JsonReader reader)
    {
        if (reader.TokenType != JsonToken.Integer)
            return null;

        var b = Convert.ToByte(reader.Value);
        reader.Read();

        return b;
    }
}
