using System;
using Bearded.TD.Commands;
using Bearded.TD.Game.Players;
using Bearded.TD.Game.Simulation.Factions;
using Bearded.TD.Game.Simulation.Technologies;
using Bearded.TD.Networking.Serialization;
using Bearded.Utilities;
using JetBrains.Annotations;

namespace Bearded.TD.Game.Commands.Debug;

static class GrantTechPoints
{
    public static IRequest<Player, GameInstance> Request(Faction faction, long number)
        => new Implementation(faction, number);

    private sealed class Implementation : UnifiedDebugRequestCommand
    {
        private readonly Faction faction;
        private readonly long number;

        public Implementation(Faction faction, long number)
        {
            this.faction = faction;
            this.number = number;
        }

        protected override bool CheckPreconditionsDebug(Player player) =>
            faction.TryGetBehavior<FactionTechnology>(out _);

        public override void Execute()
        {
            if (!faction.TryGetBehavior<FactionTechnology>(out var technology))
            {
                throw new InvalidOperationException(
                    "Cannot add tech points without technology for the faction. Precondition should have failed.");
            }

            technology.AddTechPoints(number);
        }

        protected override UnifiedRequestCommandSerializer GetSerializer() => new Serializer(faction, number);
    }

    private sealed class Serializer : UnifiedRequestCommandSerializer
    {
        private Id<Faction> faction;
        private long number;

        [UsedImplicitly]
        public Serializer() { }

        public Serializer(Faction faction, long number)
        {
            this.faction = faction.Id;
            this.number = number;
        }

        protected override UnifiedRequestCommand GetSerialized(GameInstance game)
            => new Implementation(game.State.Factions.Resolve(faction), number);

        public override void Serialize(INetBufferStream stream)
        {
            stream.Serialize(ref faction);
            stream.Serialize(ref number);
        }
    }
}