using Bearded.TD.Commands;
using Bearded.TD.Commands.Serialization;
using Bearded.TD.Content.Mods;
using Bearded.TD.Game.Simulation.Buildings;
using Bearded.TD.Game.Simulation.Upgrades;
using Bearded.TD.Networking.Serialization;
using Bearded.Utilities;
using JetBrains.Annotations;

namespace Bearded.TD.Game.Commands.Gameplay
{
    static class FinishBuildingUpgrade
    {
        public static ISerializableCommand<GameInstance> Command(Building building, IUpgradeBlueprint upgrade)
            => new Implementation(building, upgrade);

        private sealed class Implementation : ISerializableCommand<GameInstance>
        {
            private readonly Building building;
            private readonly IUpgradeBlueprint upgrade;

            public Implementation(Building building, IUpgradeBlueprint upgrade)
            {
                this.building = building;
                this.upgrade = upgrade;
            }

            public void Execute() => building.ApplyUpgrade(upgrade);

            public ICommandSerializer<GameInstance> Serializer => new Serializer(building, upgrade);
        }

        private sealed class Serializer : ICommandSerializer<GameInstance>
        {
            private Id<Building> building;
            private ModAwareId upgrade;

            [UsedImplicitly]
            public Serializer() { }

            public Serializer(Building building, IUpgradeBlueprint upgrade)
            {
                this.building = building.Id;
                this.upgrade = upgrade.Id;
            }

            public ISerializableCommand<GameInstance> GetCommand(GameInstance game)
                => new Implementation(game.State.Find(building), game.Blueprints.Upgrades[upgrade]);

            public void Serialize(INetBufferStream stream)
            {
                stream.Serialize(ref building);
                stream.Serialize(ref upgrade);
            }
        }
    }
}
