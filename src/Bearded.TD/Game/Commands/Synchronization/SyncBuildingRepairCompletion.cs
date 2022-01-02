using System;
using Bearded.TD.Commands;
using Bearded.TD.Commands.Serialization;
using Bearded.TD.Game.Simulation.Buildings.Ruins;
using Bearded.TD.Game.Simulation.Components;
using Bearded.TD.Game.Simulation.GameObjects;
using Bearded.TD.Networking.Serialization;
using Bearded.Utilities;
using JetBrains.Annotations;

namespace Bearded.TD.Game.Commands.Synchronization;

static class SyncBuildingRepairCompletion
{
    public static ISerializableCommand<GameInstance> Command(ComponentGameObject building)
        => new Implementation(building);

    private sealed class Implementation : ISerializableCommand<GameInstance>
    {
        private readonly ComponentGameObject building;

        public Implementation(ComponentGameObject building)
        {
            this.building = building;
        }

        public void Execute()
        {
            if (!building.TryGetSingleComponent<IRepairSyncer>(out var repairSyncer))
            {
                throw new InvalidOperationException(
                    "Cannot sync building repair completion on a building without construction syncer.");
            }
            repairSyncer.SyncCompleteRepair();
        }

        ICommandSerializer<GameInstance> ISerializableCommand<GameInstance>.Serializer => new Serializer(building);
    }

    private sealed class Serializer : ICommandSerializer<GameInstance>
    {
        private Id<ComponentGameObject> building;

        public Serializer(ComponentGameObject building)
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