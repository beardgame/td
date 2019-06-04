using Bearded.TD.Commands;
using Bearded.TD.Game.Factions;
using Bearded.TD.Networking.Serialization;
using Bearded.Utilities;

namespace Bearded.TD.Game.Commands.Debug
{
    static class GrantResources
    {
        public static IRequest<GameInstance> Request(Faction faction, double amount)
            => new Implementation(faction, amount);

        private class Implementation : UnifiedDebugRequestCommand
        {
            private readonly Faction faction;
            private readonly double amount;

            public Implementation(Faction faction, double amount)
            {
                this.faction = faction;
                this.amount = amount;
            }

            protected override bool CheckPreconditionsDebug() => faction.HasResources;

            public override void Execute() => faction.Resources.ProvideOneTimeResource(amount);

            protected override UnifiedRequestCommandSerializer GetSerializer() => new Serializer(faction, amount);
        }

        private class Serializer : UnifiedRequestCommandSerializer
        {
            private Id<Faction> faction;
            private double amount;
            
            // ReSharper disable once UnusedMember.Local
            public Serializer() { }

            public Serializer(Faction faction, double amount)
            {
                this.faction = faction.Id;
                this.amount = amount;
            }

            protected override UnifiedRequestCommand GetSerialized(GameInstance game)
                => new Implementation(game.State.FactionFor(faction), amount);

            public override void Serialize(INetBufferStream stream)
            {
                stream.Serialize(ref faction);
                stream.Serialize(ref amount);
            }
        }
    }
}
