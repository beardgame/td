using System;
using Newtonsoft.Json;

namespace Bearded.TD.Content.Serialization.Converters;

sealed class InterfaceConverter : JsonConverter
{
    private readonly Type interfaceType;
    private readonly Type actualType;

    public InterfaceConverter(Type interfaceType, Type actualType)
    {
        this.interfaceType = interfaceType;
        this.actualType = actualType;
    }

    public override bool CanWrite => false;

    public override bool CanConvert(Type type)
        => type == interfaceType;

    public override object ReadJson(JsonReader reader, Type type, object existingValue, JsonSerializer serializer)
        => serializer.Deserialize(reader, actualType)!;

    public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        => throw new InvalidOperationException();
}
