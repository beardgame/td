using System;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Bearded.TD.Content.Serialization.Converters
{
    abstract class JsonConverterBase<T> : JsonConverter<T>
    {
        private static readonly Type nullableType = typeof(T).IsValueType
            ? typeof(Nullable<>).MakeGenericType(typeof(T))
            : typeof(T);

        public override bool CanConvert(Type objectType)
            => typeof(T).IsAssignableFrom(objectType) || objectType == nullableType;

        public override T? Read(
            ref Utf8JsonReader reader,
            Type typeToConvert,
            JsonSerializerOptions options)
        {
            if (reader.TokenType == JsonTokenType.Null)
            {
                if (typeToConvert == nullableType)
                {
                    return default;
                }

                throw new InvalidDataException($"Cannot convert null value to {typeToConvert}.");
            }

            return ReadJson(ref reader, options);
        }

        public override void Write(
            Utf8JsonWriter writer,
            T? value,
            JsonSerializerOptions options)
        {
            if (value == null)
            {
                throw new ArgumentNullException(nameof(value));
            }

            WriteJson(writer, (T) value, options);
        }

        protected abstract T ReadJson(ref Utf8JsonReader reader, JsonSerializerOptions options);

        protected virtual void WriteJson(Utf8JsonWriter writer, T value, JsonSerializerOptions options)
        {
            throw new InvalidOperationException($"Cannot serialise {typeof(T).FullName}.");
        }
    }
}
