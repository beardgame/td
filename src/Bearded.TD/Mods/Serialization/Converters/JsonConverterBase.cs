using System;
using System.IO;
using Newtonsoft.Json;

namespace Bearded.TD.Mods.Serialization.Converters
{
    public abstract class JsonConverterBase<T> : JsonConverter
    {
        private static readonly Type nullableType = typeof(T).IsValueType
            ? typeof(Nullable<>).MakeGenericType(typeof(T))
            : typeof(T);

        public override bool CanConvert(Type objectType)
            => typeof(T).IsAssignableFrom(objectType) || objectType == nullableType;

        public override object ReadJson(JsonReader reader, Type objectType,
            object existingValue, JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.Null)
            {
                if (objectType == nullableType)
                    return null;

                throw new InvalidDataException($"Cannot convert null value to {objectType}.");
            }

            return ReadJson(reader, serializer);
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            if (value == null)
                throw new ArgumentNullException(nameof(value));

            if (!(value is T))
                throw new ArgumentException(
                    $"Unexpected value when converting. Expected {typeof(T).FullName}, got {value.GetType().FullName}."
                );

            WriteJson(writer, (T) value, serializer);
        }

        protected abstract T ReadJson(JsonReader reader, JsonSerializer serializer);

        protected virtual void WriteJson(JsonWriter writer, T value, JsonSerializer serializer)
        {
            throw new NotImplementedException($"Cannot serialise {typeof(T).FullName}.");
        }
    }
}
