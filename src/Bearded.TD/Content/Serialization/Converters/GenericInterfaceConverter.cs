using System;
using Newtonsoft.Json;

namespace Bearded.TD.Content.Serialization.Converters;

sealed class GenericInterfaceConverter : JsonConverter
{
    private readonly Type interfaceType;
    private readonly Type actualType;

    public static GenericInterfaceConverter From(Type interfaceType, Type actualType)
    {
        if (!interfaceType.IsInterface)
            throw new ArgumentException("Type must be an interface.", nameof(interfaceType));
        if (!interfaceType.IsGenericType)
            throw new ArgumentException($"{nameof(interfaceType)} must be a generic type", nameof(interfaceType));
        if(!actualType.IsGenericType)
            throw new ArgumentException($"{nameof(actualType)} must be a generic type", nameof(actualType));
        if(interfaceType.GenericTypeArguments.Length != actualType.GenericTypeArguments.Length)
            throw new ArgumentException("The number of generic arguments must be equal", nameof(actualType));

        return new GenericInterfaceConverter(interfaceType, actualType);
    }

    private GenericInterfaceConverter(Type interfaceType, Type actualType)
    {
        this.interfaceType = interfaceType;
        this.actualType = actualType;
    }

    public override bool CanWrite => false;

    public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
        => throw new NotImplementedException();

    public override object? ReadJson(JsonReader reader, Type type, object? existingValue, JsonSerializer serializer)
    {
        var realType = actualType.MakeGenericType(type.GenericTypeArguments);
        return serializer.Deserialize(reader, realType);
    }

    public override bool CanConvert(Type type) =>
        type.IsInterface && type.IsGenericType && type.GetGenericTypeDefinition() == interfaceType;
}
