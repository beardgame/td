using Bearded.TD.Commands;
using Bearded.TD.Commands.Serialization;
using Bearded.TD.Game.Simulation.Buildings.Ruins;
using Bearded.TD.Game.Simulation.GameObjects;
using Bearded.TD.Networking.Serialization;
using Bearded.Utilities;
using JetBrains.Annotations;

namespace Bearded.TD.Game.Simulation.Buildings;

static class RuinBuilding
{
    public static ISerializableCommand<GameInstance> Command(GameObject building)
        => new Implementation(building);

    private sealed class Implementation : ISerializableCommand<GameInstance>
    {
        private readonly GameObject building;

        public Implementation(GameObject building)
        {
            this.building = building;
        }

        public void Execute()
        {
            building.AddComponent(new Ruined());
        }

        ICommandSerializer<GameInstance> ISerializableCommand<GameInstance>.Serializer => new Serializer(building);
    }

    private sealed class Serializer : ICommandSerializer<GameInstance>
    {
        private Id<GameObject> building;

        [UsedImplicitly]
        public Serializer() { }

        public Serializer(GameObject building)
        {
            this.building = building.FindId();
        }

        public ISerializableCommand<GameInstance> GetCommand(GameInstance game)
            => new Implementation(game.State.Find(building));

        public void Serialize(INetBufferStream stream)
        {
            stream.Serialize(ref building);
        }
    }
}
