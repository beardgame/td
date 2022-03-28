using System.Linq;
using Bearded.TD.Commands;
using Bearded.TD.Content.Mods;
using Bearded.TD.Game.Commands;
using Bearded.TD.Game.Players;
using Bearded.TD.Game.Simulation.Buildings;
using Bearded.TD.Game.Simulation.Components;
using Bearded.TD.Game.Simulation.GameObjects;
using Bearded.TD.Game.Simulation.Technologies;
using Bearded.TD.Networking.Serialization;
using Bearded.Utilities;
using JetBrains.Annotations;

namespace Bearded.TD.Game.Simulation.Upgrades;

static class UpgradeBuilding
{
    public static IRequest<Player, GameInstance> Request(
        GameInstance game, ComponentGameObject building, IUpgradeBlueprint upgrade)
        => new Implementation(game, building, upgrade);

    private sealed class Implementation : UnifiedRequestCommand
    {
        private readonly GameInstance game;
        private readonly ComponentGameObject building;
        private readonly IUpgradeBlueprint upgrade;

        public Implementation(GameInstance game, ComponentGameObject building, IUpgradeBlueprint upgrade)
        {
            this.game = game;
            this.building = building;
            this.upgrade = upgrade;
        }

        public override bool CheckPreconditions(Player actor)
        {
            if (!actor.Faction.TryGetBehaviorIncludingAncestors<FactionTechnology>(out var technology))
            {
                return false;
            }
            if (building.GetComponents<IBuildingUpgradeManager>().SingleOrDefault() is not { } upgradeManager)
            {
                return false;
            }
            return technology.IsUpgradeUnlocked(upgrade)
                && upgradeManager.CanApplyUpgrade(upgrade)
                && upgradeManager.CanBeUpgradedBy(actor.Faction);
        }

        public override void Execute()
        {
            var upgradeManager = building.GetComponents<IBuildingUpgradeManager>().Single();
            var incompleteUpgrade = upgradeManager.QueueUpgrade(upgrade);
            building.AddComponent(new BuildingUpgradeWork(incompleteUpgrade));
        }

        public override ISerializableCommand<GameInstance> ToCommand() =>
            new Implementation(game, building, upgrade);

        protected override UnifiedRequestCommandSerializer GetSerializer() => new Serializer(building, upgrade);
    }

    private sealed class Serializer : UnifiedRequestCommandSerializer
    {
        private Id<ComponentGameObject> building;
        private ModAwareId upgrade;

        [UsedImplicitly]
        public Serializer() { }

        public Serializer(ComponentGameObject building, IUpgradeBlueprint upgrade)
        {
            this.building = building.FindId();
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
