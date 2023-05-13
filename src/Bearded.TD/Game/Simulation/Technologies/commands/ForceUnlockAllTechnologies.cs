using System;
using System.Collections.Immutable;
using System.Linq;
using Bearded.TD.Commands;
using Bearded.TD.Game.Commands;
using Bearded.TD.Game.Players;
using Bearded.TD.Game.Simulation.Factions;
using Bearded.TD.Networking.Serialization;
using Bearded.Utilities;
using JetBrains.Annotations;

namespace Bearded.TD.Game.Simulation.Technologies;

static class ForceUnlockAllTechnologies
{
    public static IRequest<Player, GameInstance> Request(GameInstance game, Faction faction) =>
        new Implementation(game, faction);

    private sealed class Implementation : UnifiedDebugRequestCommand
    {
        private readonly GameInstance game;
        private readonly Faction faction;

        public Implementation(GameInstance game, Faction faction)
        {
            this.game = game;
            this.faction = faction;
        }

        public override void Execute()
        {
            if (!faction.TryGetBehaviorIncludingAncestors<FactionTechnology>(out var factionTechnology))
            {
                throw new InvalidOperationException(
                    "Cannot replace technology queue without technology for the faction. " +
                    "Precondition should have failed.");
            }

            var lockedTechnologies =
                game.Blueprints.Technologies.All.Where(factionTechnology.IsTechnologyLocked).ToImmutableArray();
            foreach (var t in lockedTechnologies)
            {
                factionTechnology.ForceUnlockTechnology(t);
            }
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

        protected override UnifiedRequestCommandSerializer GetSerializer() => new Serializer(faction);
    }

    private sealed class Serializer : UnifiedRequestCommandSerializer
    {
        private Id<Faction> faction;

        [UsedImplicitly]
        public Serializer()
        {
        }

        public Serializer(Faction faction)
        {
            this.faction = faction.Id;
        }

        protected override UnifiedRequestCommand GetSerialized(GameInstance game) =>
            new Implementation(game, game.State.Factions.Resolve(faction));

        public override void Serialize(INetBufferStream stream)
        {
            stream.Serialize(ref faction);
        }
    }
}
