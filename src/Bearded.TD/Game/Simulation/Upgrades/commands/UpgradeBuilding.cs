using System.Linq;
using Bearded.TD.Commands;
using Bearded.TD.Content.Mods;
using Bearded.TD.Game.Commands;
using Bearded.TD.Game.Players;
using Bearded.TD.Game.Simulation.Buildings;
using Bearded.TD.Game.Simulation.Buildings.Ruins;
using Bearded.TD.Game.Simulation.GameObjects;
using Bearded.TD.Game.Simulation.Resources;
using Bearded.TD.Game.Simulation.Technologies;
using Bearded.TD.Networking.Serialization;
using Bearded.TD.Utilities;
using Bearded.Utilities;
using JetBrains.Annotations;

namespace Bearded.TD.Game.Simulation.Upgrades;

static class UpgradeBuilding
{
    public static IRequest<Player, GameInstance> Request(
        GameInstance game, GameObject building, IPermanentUpgrade upgrade)
        => new Implementation(game, building, upgrade);

    private sealed class Implementation : UnifiedRequestCommand
    {
        private readonly GameInstance game;
        private readonly GameObject building;
        private readonly IPermanentUpgrade upgrade;

        public Implementation(GameInstance game, GameObject building, IPermanentUpgrade upgrade)
        {
            this.game = game;
            this.building = building;
            this.upgrade = upgrade;
        }

        public override bool CheckPreconditions(Player actor)
        {
            if (!actor.Faction.TryGetBehaviorIncludingAncestors<FactionTechnology>(out var technology) ||
                !actor.Faction.TryGetBehaviorIncludingAncestors<FactionResources>(out var resources))
            {
                return false;
            }
            if (building.GetComponents<IUpgradeSlots>().SingleOrDefault() is not { } upgradeSlots ||
                building.GetComponents<IBuildingUpgradeManager>().SingleOrDefault() is not { } upgradeManager)
            {
                return false;
            }
            return technology.IsUpgradeUnlocked(upgrade)
                && resources.GetCurrent<Scrap>() >= upgrade.Cost
                && upgradeSlots.HasAvailableSlot
                && upgradeManager.CanApplyUpgrade(upgrade)
                && upgradeManager.CanBeUpgradedBy(actor.Faction);
        }

        public override void Execute()
        {
            var upgradeSlots = building.GetComponents<IUpgradeSlots>().Single();
            upgradeSlots.FillSlot(upgrade);

            building.FindFaction().TryGetBehaviorIncludingAncestors<FactionResources>(out var resources);
            resources!.ConsumeResources(upgrade.Cost);

            if (building.GetComponents<IBreakageHandler>().SingleOrDefault() is { } breakageHandler)
            {
                var receipt = breakageHandler.BreakObject();
                building.Delay(receipt.Repair, 5.S());
            }
        }

        public override ISerializableCommand<GameInstance> ToCommand() =>
            new Implementation(game, building, upgrade);

        protected override UnifiedRequestCommandSerializer GetSerializer() => new Serializer(building, upgrade);
    }

    private sealed class Serializer : UnifiedRequestCommandSerializer
    {
        private Id<GameObject> building;
        private ModAwareId upgrade;

        [UsedImplicitly]
        public Serializer() { }

        public Serializer(GameObject building, IPermanentUpgrade upgrade)
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
