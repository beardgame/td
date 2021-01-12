using System;
using System.IO;
using System.Text.Json;
using amulware.Graphics;

namespace Bearded.TD.Content.Serialization.Converters
{
    sealed class ColorConverter : JsonConverterBase<Color>
    {
        protected override Color ReadJson(ref Utf8JsonReader reader, JsonSerializerOptions options)
        {
            if (reader.TokenType == JsonTokenType.String)
            {
                var s = reader.GetString();

                foreach (var field in typeof(Color).GetFields())
                {
                    if (field.IsStatic
                        && field.FieldType == typeof(Color)
                        && field.Name.Equals(s, StringComparison.InvariantCultureIgnoreCase))
                    {
                        return (Color) field.GetValue(null);
                    }
                }

                try
                {
                    return new Color(Convert.ToUInt32(s, 16));
                }
                catch (Exception)
                {
                    throw new InvalidDataException("Color has unknown or invalid string value.");
                }
            }

            if (reader.TokenType == JsonTokenType.StartArray)
            {
                reader.Read();

                var r = readByte(ref reader);
                var g = readByte(ref reader);
                var b = readByte(ref reader);
                var a = reader.TryGetByte(out var alpha) ? alpha : (byte) 255;

                if (reader.TokenType == JsonTokenType.EndArray)
                {
                    return new Color(r, g, b, a);
                }
            }

            throw new JsonException("Color has no or invalid value.");
        }

        private static byte readByte(ref Utf8JsonReader reader)
        {
            if (!reader.TryGetByte(out var b))
            {
                throw new JsonException(
                    "Expected number value, " +
                    $"encountered {reader.TokenType} when parsing Color component (expecting byte).");
            }

            return b;
        }
    }
}
