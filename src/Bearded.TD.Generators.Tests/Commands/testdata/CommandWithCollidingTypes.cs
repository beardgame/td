using System;
using Bearded.TD.Game;
using Bearded.TD.Shared.Commands;

namespace Bearded.TD.Generators.Tests.Commands;

[GenerateCommand]
static partial class CommandWithCollidingTypes
{
    private static void execute(CollidingType one, OtherType.CollidingType two, OtherType2.CollidingType three)
    {
        throw new NotImplementedException(":)");
    }

    public sealed class CollidingType;

    public sealed class OtherType
    {
        public sealed class CollidingType;
    }

    public sealed class OtherType2
    {
        public sealed class CollidingType;
    }

    public static class CollidingTypeSerializerConverters
    {
        [SerializerConverter]
        public static readonly ISerializerConverter<OtherType2.CollidingType, int, GameInstance> SerializeCollidingType =
            new SerializerConverter<OtherType2.CollidingType, int, GameInstance>(_ => 0, (_, _) => new OtherType2.CollidingType());

    }
}

static class CollidingTypeSerializerConverters
{
    [SerializerConverter]
    public static readonly ISerializerConverter<CommandWithCollidingTypes.CollidingType, int, GameInstance> SerializeCollidingType =
        new SerializerConverter<CommandWithCollidingTypes.CollidingType, int, GameInstance>(_ => 0, (_, _) => new CommandWithCollidingTypes.CollidingType());

    [SerializerConverter]
    public static readonly ISerializerConverter<CommandWithCollidingTypes.OtherType.CollidingType, int, GameInstance> SerializeOtherTypeCollidingType =
        new SerializerConverter<CommandWithCollidingTypes.OtherType.CollidingType, int, GameInstance>(_ => 0, (_, _) => new CommandWithCollidingTypes.OtherType.CollidingType());
}
