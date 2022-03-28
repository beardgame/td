using Bearded.TD.Commands;
using Bearded.TD.Commands.Serialization;
using Bearded.TD.Game.Simulation.Components;
using Bearded.TD.Networking.Serialization;
using Bearded.Utilities;
using JetBrains.Annotations;

namespace Bearded.TD.Game.Simulation.GameObjects;

static class DeleteGameObject
{
    public static ISerializableCommand<GameInstance> Command(GameObject gameObject) =>
        new Implementation(gameObject);

    private sealed class Implementation : ISerializableCommand<GameInstance>
    {
        private readonly GameObject gameObject;

        public Implementation(GameObject gameObject)
        {
            this.gameObject = gameObject;
        }

        public void Execute() => gameObject.Delete();
        ICommandSerializer<GameInstance> ISerializableCommand<GameInstance>.Serializer =>
            new Serializer(gameObject);
    }

    private sealed class Serializer : ICommandSerializer<GameInstance>
    {
        private Id<GameObject> gameObject;

        [UsedImplicitly]
        public Serializer() {}

        public Serializer(GameObject gameObject)
        {
            this.gameObject = gameObject.FindId();
        }

        public ISerializableCommand<GameInstance> GetCommand(GameInstance game)
        {
            return new Implementation(game.State.Find(gameObject));
        }

        public void Serialize(INetBufferStream stream)
        {
            stream.Serialize(ref gameObject);
        }
    }
}
