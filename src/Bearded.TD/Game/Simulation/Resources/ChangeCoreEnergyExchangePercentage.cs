using System;
using Bearded.TD.Commands;
using Bearded.TD.Content.Mods;
using Bearded.TD.Game.Commands;
using Bearded.TD.Game.Players;
using Bearded.TD.Game.Simulation.Factions;
using Bearded.TD.Game.Simulation.Technologies;
using Bearded.TD.Networking.Serialization;
using Bearded.Utilities;
using JetBrains.Annotations;

namespace Bearded.TD.Game.Simulation.Resources;

static class ChangeCoreEnergyExchangePercentage
{
    public static IRequest<Player, GameInstance> Request(Faction faction, double percentage)
        => new Implementation(faction, percentage);

    private sealed class Implementation(Faction faction, double percentage) : UnifiedRequestCommand
    {
        public override void Execute()
        {
            if (!faction.TryGetBehaviorIncludingAncestors<FactionCoreEnergyExchange>(out var exchange))
            {
                throw new InvalidOperationException(
                    "Cannot replace technology queue without technology for the faction. " +
                    "Precondition should have failed.");
            }
            exchange.SetExchangePercentage(percentage);
        }

        public override bool CheckPreconditions(Player actor)
        {
            return faction.TryGetBehaviorIncludingAncestors<FactionCoreEnergyExchange>(out _);
        }

        protected override UnifiedRequestCommandSerializer GetSerializer() => new Serializer(faction, percentage);
    }

    private sealed class Serializer : UnifiedRequestCommandSerializer
    {
        private Id<Faction> faction;
        private double percentage;

        [UsedImplicitly]
        public Serializer()
        {
        }

        public Serializer(Faction faction, double percentage)
        {
            this.faction = faction.Id;
            this.percentage = percentage;
        }

        protected override UnifiedRequestCommand GetSerialized(GameInstance game) =>
            new Implementation(game.State.Factions.Resolve(faction), percentage);

        public override void Serialize(INetBufferStream stream)
        {
            stream.Serialize(ref faction);
            stream.Serialize(ref percentage);
        }
    }
}
