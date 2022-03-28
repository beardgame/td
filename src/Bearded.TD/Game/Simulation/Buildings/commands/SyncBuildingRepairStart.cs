using System;
using Bearded.TD.Commands;
using Bearded.TD.Commands.Serialization;
using Bearded.TD.Game.Simulation.Buildings.Ruins;
using Bearded.TD.Game.Simulation.Components;
using Bearded.TD.Game.Simulation.GameObjects;
using Bearded.TD.Networking.Serialization;
using Bearded.Utilities;
using JetBrains.Annotations;

namespace Bearded.TD.Game.Simulation.Buildings;

static class SyncBuildingRepairStart
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
            if (!building.TryGetSingleComponent<IRepairSyncer>(out var repairSyncer))
            {
                throw new InvalidOperationException(
                    "Cannot sync building repair start on a building without construction syncer.");
            }
            repairSyncer.SyncStartRepair();
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

        [UsedImplicitly]
        public Serializer() { }

        public ISerializableCommand<GameInstance> GetCommand(GameInstance game) =>
            new Implementation(game.State.Find(building));

        public void Serialize(INetBufferStream stream)
        {
            stream.Serialize(ref building);
        }
    }
}
