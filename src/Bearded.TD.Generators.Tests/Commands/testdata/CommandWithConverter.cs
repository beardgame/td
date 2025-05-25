using System;
using Bearded.TD.Game;
using Bearded.TD.Shared.Commands;

namespace Bearded.TD.Generators.Tests.Commands;

[GenerateCommand]
static partial class CommandWithConverter
{
    private static void execute(MyType myType)
    {
        throw new NotImplementedException(":)");
    }
}

sealed class MyType;

static class SerializerConverters
{
    [SerializerConverter]
    public static readonly ISerializerConverter<MyType, int, GameInstance> SerializeMyType =
        new SerializerConverter<MyType, int, GameInstance>(_ => 0, (_, _) => new MyType());
}
