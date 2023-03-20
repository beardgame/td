using System.Linq;
using Bearded.TD.Commands;
using Bearded.TD.Commands.Serialization;
using Bearded.TD.Game.Simulation.GameObjects;
using Bearded.TD.Networking.Serialization;
using Bearded.Utilities;
using Bearded.Utilities.SpaceTime;
using JetBrains.Annotations;

namespace Bearded.TD.Game.Simulation.Modules;

static class TeleportGameObject
{
    public static ISerializableCommand<GameInstance> Command(GameObject obj, Position3 position) =>
        new Implementation(obj, position);

    private sealed class Implementation : ISerializableCommand<GameInstance>
    {
        private readonly GameObject obj;
        private readonly Position3 position;

        public Implementation(GameObject obj, Position3 position)
        {
            this.obj = obj;
            this.position = position;
        }

        public void Execute() => obj.GetComponents<ITeleporter>().Single().Teleport(position);

        ICommandSerializer<GameInstance> ISerializableCommand<GameInstance>.Serializer =>
            new Serializer(obj, position);
    }

    private sealed class Serializer : ICommandSerializer<GameInstance>
    {
        private Id<GameObject> obj;
        private Position3 position;

        [UsedImplicitly] public Serializer() { }

        public Serializer(GameObject obj, Position3 position)
        {
            this.obj = obj.FindId();
            this.position = position;
        }

        public ISerializableCommand<GameInstance> GetCommand(GameInstance game)
        {
            return new Implementation(game.State.Find(obj), position);
        }

        public void Serialize(INetBufferStream stream)
        {
            stream.Serialize(ref obj);
            stream.Serialize(ref position);
        }
    }
}
