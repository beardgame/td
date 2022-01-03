using Bearded.TD.Commands;
using Bearded.TD.Commands.Serialization;
using Bearded.TD.Game.Simulation.Components;
using Bearded.TD.Networking.Serialization;

namespace Bearded.TD.Game.Loading;

static class InitializeTypes
{
    public static ISerializableCommand<GameInstance> Command()
        => new Implementation();

    private sealed class Implementation : ISerializableCommand<GameInstance>
    {
        // ReSharper disable once EmptyConstructor
        public Implementation() {}

        public void Execute()
        {
            ParametersTemplateLibrary.TouchModifiableClasses();
        }

        ICommandSerializer<GameInstance> ISerializableCommand<GameInstance>.Serializer => new Serializer();
    }

    private sealed class Serializer : ICommandSerializer<GameInstance>
    {
        // ReSharper disable once EmptyConstructor
        public Serializer() { }

        public ISerializableCommand<GameInstance> GetCommand(GameInstance game) => new Implementation();

        public void Serialize(INetBufferStream stream) { }
    }
}