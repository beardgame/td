using System;
using System.Linq;
using Bearded.TD.Commands;
using Bearded.TD.Commands.Serialization;
using Bearded.TD.Content.Mods;
using Bearded.TD.Game.Simulation.Buildings;
using Bearded.TD.Game.Simulation.GameObjects;
using Bearded.TD.Networking.Serialization;
using Bearded.Utilities;
using JetBrains.Annotations;

namespace Bearded.TD.Game.Simulation.Upgrades;

static class SyncUpgradeStart
{
    public static ISerializableCommand<GameInstance> Command(GameObject building, ModAwareId upgradeId) =>
        new Implementation(building, upgradeId);

    private sealed class Implementation : ISerializableCommand<GameInstance>
    {
        private readonly GameObject building;
        private readonly ModAwareId upgradeId;

        public Implementation(GameObject building, ModAwareId upgradeId)
        {
            this.building = building;
            this.upgradeId = upgradeId;
        }

        public void Execute()
        {
            var upgradeSyncer = building.GetComponents<IBuildingUpgradeSyncer>().SingleOrDefault()
                ?? throw new InvalidOperationException(
                    "Cannot sync building upgrade start on a building without building upgrade syncer.");
            upgradeSyncer.SyncStartUpgrade(upgradeId);
        }

        ICommandSerializer<GameInstance> ISerializableCommand<GameInstance>.Serializer => new Serializer(building, upgradeId);
    }

    private sealed class Serializer : ICommandSerializer<GameInstance>
    {
        private Id<GameObject> building;
        private ModAwareId upgradeId;

        [UsedImplicitly]
        public Serializer() { }

        public Serializer(GameObject building, ModAwareId upgradeId)
        {
            this.building = building.FindId();
            this.upgradeId = upgradeId;
        }

        public ISerializableCommand<GameInstance> GetCommand(GameInstance game) =>
            new Implementation(game.State.Find(building), upgradeId);

        public void Serialize(INetBufferStream stream)
        {
            stream.Serialize(ref building);
            stream.Serialize(ref upgradeId);
        }
    }
}
