using System;
using Bearded.TD.Commands;
using Bearded.TD.Game.Players;
using Bearded.TD.Game.Simulation.Factions;
using Bearded.TD.Game.Simulation.Technologies;
using Bearded.TD.Networking.Serialization;
using Bearded.Utilities;
using JetBrains.Annotations;

namespace Bearded.TD.Game.Commands.Gameplay
{
    static class ClearTechnologyQueue
    {
        public static IRequest<Player, GameInstance> Request(Faction faction) => new Implementation(faction);

        private sealed class Implementation : UnifiedRequestCommand
        {
            private readonly Faction faction;

            public Implementation(Faction faction)
            {
                this.faction = faction;
            }

            public override bool CheckPreconditions(Player actor)
            {
                if (!faction.TryGetBehaviorIncludingAncestors<FactionTechnology>(out var factionTechnology))
                {
                    return false;
                }
                actor.Faction.TryGetBehaviorIncludingAncestors<FactionTechnology>(out var actorTechnology);
                return factionTechnology == actorTechnology;
            }

            public override void Execute()
            {
                if (!faction.TryGetBehaviorIncludingAncestors<FactionTechnology>(out var factionTechnology))
                {
                    throw new InvalidOperationException(
                        "Cannot clear technology queue without technology for the faction. " +
                        "Precondition should have failed.");
                }
                factionTechnology.ClearTechnologyQueue();
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
                new Implementation(game.State.Factions.Resolve(faction));

            public override void Serialize(INetBufferStream stream)
            {
                stream.Serialize(ref faction);
            }
        }
    }
}
