using System.Linq;
using Bearded.TD.Commands;
using Bearded.TD.Commands.Serialization;
using Bearded.TD.Game.Simulation.GameObjects;
using Bearded.TD.Networking.Serialization;
using Bearded.Utilities;
using JetBrains.Annotations;

namespace Bearded.TD.Game.Simulation.Buildings.Veterancy;

static class LevelUpGameObject
{
    public static ISerializableCommand<GameInstance> Command(GameObject obj) =>
        new Implementation(obj);

    private sealed class Implementation : ISerializableCommand<GameInstance>
    {
        private readonly GameObject obj;

        public Implementation(GameObject obj)
        {
            this.obj = obj;
        }

        public void Execute()
            => obj.GetComponents<ILevelable>().Single().LevelUp();

        ICommandSerializer<GameInstance> ISerializableCommand<GameInstance>.Serializer
            => new Serializer(obj);
    }

    private sealed class Serializer : ICommandSerializer<GameInstance>
    {
        private Id<GameObject> obj;

        [UsedImplicitly] public Serializer() { }

        public Serializer(GameObject obj)
            => this.obj = obj.FindId();

        public ISerializableCommand<GameInstance> GetCommand(GameInstance game)
            => new Implementation(game.State.Find(obj));

        public void Serialize(INetBufferStream stream)
            => stream.Serialize(ref obj);
    }
}
