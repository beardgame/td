using System;
using Bearded.TD.Commands;
using Bearded.TD.Content.Mods;
using Bearded.TD.Game.Commands;
using Bearded.TD.Game.Players;
using Bearded.TD.Game.Simulation.Factions;
using Bearded.TD.Networking.Serialization;
using Bearded.Utilities;
using JetBrains.Annotations;

namespace Bearded.TD.Game.Simulation.Technologies;

static class ForceUnlockTechnology
{
    public static IRequest<Player, GameInstance> Request(Faction faction, ITechnologyBlueprint technology)
        => new Implementation(faction, technology);

    private sealed class Implementation : UnifiedDebugRequestCommand
    {
        private readonly Faction faction;
        private readonly ITechnologyBlueprint technology;

        public Implementation(Faction faction, ITechnologyBlueprint technology)
        {
            this.faction = faction;
            this.technology = technology;
        }

        public override void Execute()
        {
            if (!faction.TryGetBehaviorIncludingAncestors<FactionTechnology>(out var factionTechnology))
            {
                throw new InvalidOperationException(
                    "Cannot replace technology queue without technology for the faction. " +
                    "Precondition should have failed.");
            }
            factionTechnology.ForceUnlockTechnology(technology);
        }

        protected override bool CheckPreconditionsDebug(Player actor)
        {
            if (!faction.TryGetBehaviorIncludingAncestors<FactionTechnology>(out var factionTechnology))
            {
                return false;
            }
            actor.Faction.TryGetBehaviorIncludingAncestors<FactionTechnology>(out var actorTechnology);
            return factionTechnology == actorTechnology;
        }

        protected override UnifiedRequestCommandSerializer GetSerializer() => new Serializer(faction, technology);
    }

    private sealed class Serializer : UnifiedRequestCommandSerializer
    {
        private Id<Faction> faction;
        private ModAwareId technology;

        [UsedImplicitly]
        public Serializer()
        {
        }

        public Serializer(Faction faction, ITechnologyBlueprint technology)
        {
            this.faction = faction.Id;
            this.technology = technology.Id;
        }

        protected override UnifiedRequestCommand GetSerialized(GameInstance game) =>
            new Implementation(game.State.Factions.Resolve(faction), game.Blueprints.Technologies[technology]);

        public override void Serialize(INetBufferStream stream)
        {
            stream.Serialize(ref faction);
            stream.Serialize(ref technology);
        }
    }
}
