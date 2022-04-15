using System;
using Bearded.TD.Commands;
using Bearded.TD.Game.Commands;
using Bearded.TD.Game.Players;
using Bearded.TD.Game.Simulation.Factions;
using Bearded.TD.Networking.Serialization;
using Bearded.Utilities;
using JetBrains.Annotations;

namespace Bearded.TD.Game.Simulation.Technologies;

static class GrantTechToken
{
    public static IRequest<Player, GameInstance> Request(Faction faction) => new Implementation(faction);

    private sealed class Implementation : UnifiedDebugRequestCommand
    {
        private readonly Faction faction;

        public Implementation(Faction faction)
        {
            this.faction = faction;
        }

        protected override bool CheckPreconditionsDebug(Player actor) =>
            faction.TryGetBehavior<FactionTechnology>(out var technology) && !technology.HasTechnologyToken;

        public override void Execute()
        {
            if (!faction.TryGetBehavior<FactionTechnology>(out var technology))
            {
                throw new InvalidOperationException();
            }
            technology.AwardTechnologyToken();
        }

        protected override UnifiedRequestCommandSerializer GetSerializer() => new Serializer();
    }

    private sealed class Serializer : UnifiedRequestCommandSerializer
    {
        private Id<Faction> faction;

        public Serializer(Faction faction)
        {
            this.faction = faction.Id;
        }

        [UsedImplicitly] public Serializer() { }

        protected override UnifiedRequestCommand GetSerialized(GameInstance game)
        {
            return new Implementation(game.State.Factions.Resolve(faction));
        }

        public override void Serialize(INetBufferStream stream)
        {
            stream.Serialize(ref faction);
        }
    }
}

