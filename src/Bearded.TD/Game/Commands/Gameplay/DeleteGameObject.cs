using Bearded.TD.Commands;
using Bearded.TD.Commands.Serialization;
using Bearded.TD.Game.Simulation.Components;
using Bearded.TD.Game.Simulation.GameObjects;
using Bearded.TD.Networking.Serialization;
using Bearded.Utilities;
using JetBrains.Annotations;

namespace Bearded.TD.Game.Commands.Gameplay
{
    static class DeleteGameObject
    {
        public static ISerializableCommand<GameInstance> Command(ComponentGameObject gameObject) =>
            new Implementation(gameObject);

        private sealed class Implementation : ISerializableCommand<GameInstance>
        {
            private readonly ComponentGameObject gameObject;

            public Implementation(ComponentGameObject gameObject)
            {
                this.gameObject = gameObject;
            }

            public void Execute() => gameObject.Delete();
            ICommandSerializer<GameInstance> ISerializableCommand<GameInstance>.Serializer =>
                new Serializer(gameObject);
        }

        private sealed class Serializer : ICommandSerializer<GameInstance>
        {
            private Id<ComponentGameObject> gameObject;

            [UsedImplicitly]
            public Serializer() {}

            public Serializer(ComponentGameObject gameObject)
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
}
