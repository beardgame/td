using Bearded.TD.Commands;
using Bearded.TD.Content.Mods;
using Bearded.TD.Game.GameState.Buildings;
using Bearded.TD.Game.GameState.Upgrades;
using Bearded.TD.Game.Players;
using Bearded.TD.Networking.Serialization;
using Bearded.Utilities;

namespace Bearded.TD.Game.Commands.Gameplay
{
    static class UpgradeBuilding
    {
        public static IRequest<Player, GameInstance> Request(
                GameInstance game, Building building, IUpgradeBlueprint upgrade)
            => new Implementation(game, building, upgrade);

        private class Implementation : UnifiedRequestCommand
        {
            private readonly GameInstance game;
            private readonly Building building;
            private readonly IUpgradeBlueprint upgrade;

            public Implementation(GameInstance game, Building building, IUpgradeBlueprint upgrade)
            {
                this.game = game;
                this.building = building;
                this.upgrade = upgrade;
            }

            public override bool CheckPreconditions(Player actor) =>
                actor.Faction.Technology.IsUpgradeUnlocked(upgrade)
                && building.CanApplyUpgrade(upgrade)
                && building.CanBeUpgradedBy(actor.Faction);

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
            private ModAwareId upgrade;

            // ReSharper disable once UnusedMember.Local
            public Serializer() { }

            public Serializer(Building building, IUpgradeBlueprint upgrade)
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
