using Bearded.TD.Commands;
using Bearded.TD.Game.Players;
using Bearded.TD.Game.Simulation.Exploration;
using Bearded.TD.Game.Simulation.Factions;
using Bearded.TD.Game.Simulation.Zones;
using Bearded.TD.Networking.Serialization;
using Bearded.Utilities;
using JetBrains.Annotations;

namespace Bearded.TD.Game.Commands.Gameplay;

static class RevealZone
{
    public static IRequest<Player, GameInstance> Request(Faction faction, Zone zone)
        => new Implementation(faction, zone);

    private sealed class Implementation : UnifiedRequestCommand
    {
        private readonly Faction faction;
        private readonly Zone zone;

        public Implementation(Faction faction, Zone zone)
        {
            this.faction = faction;
            this.zone = zone;
        }

        public override bool CheckPreconditions(Player actor) =>
            actor.Faction.TryGetBehaviorIncludingAncestors<FactionVisibility>(out var visibility) &&
            actor.Faction.TryGetBehaviorIncludingAncestors<FactionExploration>(out var exploration) &&
            exploration.HasExplorationToken &&
            !visibility[zone].IsRevealed();

        public override void Execute()
        {
            faction.TryGetBehaviorIncludingAncestors<FactionVisibility>(out var visibility);
            faction.TryGetBehaviorIncludingAncestors<FactionExploration>(out var exploration);
            visibility!.RevealZone(zone);
            exploration!.ConsumeExplorationToken();
        }

        protected override UnifiedRequestCommandSerializer GetSerializer() => new Serializer(faction, zone);
    }

    private sealed class Serializer : UnifiedRequestCommandSerializer
    {
        private Id<Faction> faction;
        private Id<Zone> zone;

        [UsedImplicitly] public Serializer() { }

        public Serializer(Faction faction, Zone zone)
        {
            this.faction = faction.Id;
            this.zone = zone.Id;
        }

        protected override UnifiedRequestCommand GetSerialized(GameInstance game) =>
            new Implementation(game.State.Factions.Resolve(faction), game.State.ZoneLayer.FindZone(zone));

        public override void Serialize(INetBufferStream stream)
        {
            stream.Serialize(ref faction);
            stream.Serialize(ref zone);
        }
    }
}
