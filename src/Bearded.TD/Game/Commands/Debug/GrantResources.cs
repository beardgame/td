using Bearded.TD.Commands;
using Bearded.TD.Game.Players;
using Bearded.TD.Game.Simulation.Factions;
using Bearded.TD.Game.Simulation.Resources;
using Bearded.TD.Networking.Serialization;
using Bearded.Utilities;
using JetBrains.Annotations;

namespace Bearded.TD.Game.Commands.Debug
{
    static class GrantResources
    {
        public static IRequest<Player, GameInstance> Request(Faction faction, ResourceAmount amount)
            => new Implementation(faction, amount);

        private sealed class Implementation : UnifiedDebugRequestCommand
        {
            private readonly Faction faction;
            private readonly ResourceAmount amount;

            public Implementation(Faction faction, ResourceAmount amount)
            {
                this.faction = faction;
                this.amount = amount;
            }

            protected override bool CheckPreconditionsDebug(Player _) => faction.HasResources;

            public override void Execute() => faction.Resources.ProvideResources(amount);

            protected override UnifiedRequestCommandSerializer GetSerializer() => new Serializer(faction, amount);
        }

        private sealed class Serializer : UnifiedRequestCommandSerializer
        {
            private Id<Faction> faction;
            private int amount;

            [UsedImplicitly]
            public Serializer() { }

            public Serializer(Faction faction, ResourceAmount amount)
            {
                this.faction = faction.Id;
                this.amount = amount.NumericValue;
            }

            protected override UnifiedRequestCommand GetSerialized(GameInstance game)
                => new Implementation(game.State.FactionFor(faction), amount.Resources());

            public override void Serialize(INetBufferStream stream)
            {
                stream.Serialize(ref faction);
                stream.Serialize(ref amount);
            }
        }
    }
}
