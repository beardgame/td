using System;
using Bearded.TD.Game;
using Bearded.TD.Shared.Commands;

namespace Bearded.TD.Generators.Tests.Commands;

[GenerateCommand]
static partial class CommandWithSerializer
{
    private static void execute(MySerializedType myType, MyConvertibleDeserializedType myConvertibleType)
    {
        throw new NotImplementedException(":)");
    }
}

sealed class MySerializedType;

sealed class MyConvertibleSerializedType;

sealed class MyConvertibleDeserializedType;

static class SomeSerializersAndConverters
{
    [Serializer]
    public static void SerializeOnly(ISerializerBufferStream stream, ref MySerializedType myType)
    {
    }

    [Serializer]
    public static void SerializeConverted(ISerializerBufferStream stream, ref MyConvertibleSerializedType myType)
    {
    }

    [SerializerConverter]
    public static readonly
        ISerializerConverter<MyConvertibleDeserializedType, MyConvertibleSerializedType, GameInstance> Converter
            = null!;
}
