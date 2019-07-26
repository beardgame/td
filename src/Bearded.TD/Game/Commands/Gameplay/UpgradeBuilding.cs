using Bearded.TD.Commands;
using Bearded.TD.Game.Buildings;
using Bearded.TD.Game.Factions;
using Bearded.TD.Game.Players;
using Bearded.TD.Game.Upgrades;
using Bearded.TD.Networking.Serialization;
using Bearded.Utilities;

namespace Bearded.TD.Game.Commands
{
    static class UpgradeBuilding
    {
        public static IRequest<Player, GameInstance> Request(GameInstance game, Building building, UpgradeBlueprint upgrade)
            => new Implementation(game, building, upgrade);

        private class Implementation : UnifiedRequestCommand
        {
            private readonly GameInstance game;
            private readonly Building building;
            private readonly UpgradeBlueprint upgrade;

            public Implementation(GameInstance game, Building building, UpgradeBlueprint upgrade)
            {
                this.game = game;
                this.building = building;
                this.upgrade = upgrade;
            }

            public override bool CheckPreconditions(Player actor) =>
                building.CanApplyUpgrade(upgrade) && building.CanBeUpgradedBy(actor.Faction);

            public override void Execute()
            {
                var upgradeTask = new BuildingUpgradeTask(building, upgrade);
                game.State.Add(upgradeTask);
            }

            protected override UnifiedRequestCommandSerializer GetSerializer() => new Serializer(building, upgrade);
        }

        private class Serializer : UnifiedRequestCommandSerializer
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

            protected override UnifiedRequestCommand GetSerialized(GameInstance game)
                => new Implementation(game, game.State.Find(building), game.Blueprints.Upgrades[upgrade]);

            public override void Serialize(INetBufferStream stream)
            {
                stream.Serialize(ref building);
                stream.Serialize(ref upgrade);
            }
        }
    }
}
