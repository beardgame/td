using Bearded.TD.Commands;
using Bearded.TD.Commands.Serialization;
using Bearded.TD.Game.Buildings;
using Bearded.TD.Game.Upgrades;
using Bearded.TD.Networking.Serialization;
using Bearded.Utilities;

namespace Bearded.TD.Game.Commands
{
    static class FinishBuildingUpgrade
    {
        public static ISerializableCommand<GameInstance> Command(Building building, UpgradeBlueprint upgrade)
            => new Implementation(building, upgrade);

        private class Implementation : ISerializableCommand<GameInstance>
        {
            private readonly Building building;
            private readonly UpgradeBlueprint upgrade;

            public Implementation(Building building, UpgradeBlueprint upgrade)
            {
                this.building = building;
                this.upgrade = upgrade;
            }

            public void Execute() => building.ApplyUpgrade(upgrade);

            public ICommandSerializer<GameInstance> Serializer => new Serializer(building, upgrade);
        }

        private class Serializer : ICommandSerializer<GameInstance>
        {
            private Id<Building> building;
            private Id<UpgradeBlueprint> upgrade;

            // ReSharper disable once UnusedMember.Local
            public Serializer() { }

            public Serializer(Building building, UpgradeBlueprint upgrade)
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
