using System;
using System.Linq;
using Bearded.TD.Commands;
using Bearded.TD.Commands.Serialization;
using Bearded.TD.Game.Simulation.GameObjects;
using Bearded.TD.Networking.Serialization;
using Bearded.Utilities;

namespace Bearded.TD.Game.Simulation.Buildings;

static class SyncBuildingConstructionStart
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
            var constructor = building.GetComponents<IBuildingConstructionSyncer>().SingleOrDefault()
                ?? throw new InvalidOperationException(
                    "Cannot sync building construction start on a building without construction syncer.");
            constructor.SyncStartBuild();
        }

        ICommandSerializer<GameInstance> ISerializableCommand<GameInstance>.Serializer => new Serializer(building);
    }

    private sealed class Serializer : ICommandSerializer<GameInstance>
    {
        private Id<GameObject> building;

        public Serializer(GameObject building)
        {
            this.building = building.FindId();
        }

        // ReSharper disable once UnusedMember.Local
        public Serializer() { }

        public ISerializableCommand<GameInstance> GetCommand(GameInstance game) =>
            new Implementation(game.State.Find(building));

        public void Serialize(INetBufferStream stream)
        {
            stream.Serialize(ref building);
        }
    }
}
