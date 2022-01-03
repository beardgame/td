using Bearded.TD.Commands;
using Bearded.TD.Game.Commands;
using Bearded.TD.Game.Players;
using Bearded.TD.Game.Simulation.Zones;
using Bearded.TD.Networking.Serialization;
using Bearded.Utilities;
using JetBrains.Annotations;

namespace Bearded.TD.Game.Simulation.Exploration;

static class RevealZone
{
    public static IRequest<Player, GameInstance> Request(GameInstance game, Zone zone)
        => new Implementation(game, zone);

    private sealed class Implementation : UnifiedRequestCommand
    {
        private readonly GameInstance game;
        private readonly Zone zone;

        public Implementation(GameInstance game, Zone zone)
        {
            this.game = game;
            this.zone = zone;
        }

        public override bool CheckPreconditions(Player actor) =>
            game.State.ExplorationManager.HasExplorationToken && !game.State.VisibilityLayer[zone].IsRevealed();

        public override void Execute()
        {
            game.State.VisibilityLayer.RevealZone(zone);
            game.State.ExplorationManager.ConsumeExplorationToken();
        }

        protected override UnifiedRequestCommandSerializer GetSerializer() => new Serializer(zone);
    }

    private sealed class Serializer : UnifiedRequestCommandSerializer
    {
        private Id<Zone> zone;

        [UsedImplicitly] public Serializer() { }

        public Serializer(Zone zone)
        {
            this.zone = zone.Id;
        }

        protected override UnifiedRequestCommand GetSerialized(GameInstance game) =>
            new Implementation(game, game.State.ZoneLayer.FindZone(zone));

        public override void Serialize(INetBufferStream stream)
        {
            stream.Serialize(ref zone);
        }
    }
}
