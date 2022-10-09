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
    public static IRequest<Player, GameInstance> Request(GameState game, Zone zone) =>
        new Implementation(game, zone, true);

    public static ISerializableCommand<GameInstance> Command(GameState game, Zone zone) =>
        new Implementation(game, zone, false);

    private sealed class Implementation : UnifiedRequestCommand
    {
        private readonly GameState game;
        private readonly Zone zone;
        private readonly bool consumeToken;

        public Implementation(GameState game, Zone zone, bool consumeToken)
        {
            this.game = game;
            this.zone = zone;
            this.consumeToken = consumeToken;
        }

        public override bool CheckPreconditions(Player actor) =>
            zone.Explorable &&
            (!consumeToken || game.ExplorationManager.HasExplorationToken) &&
            !game.VisibilityLayer[zone].IsRevealed();

        public override void Execute()
        {
            game.VisibilityLayer.RevealZone(zone);
            if (consumeToken)
            {
                game.ExplorationManager.ConsumeExplorationToken();
            }
        }

        protected override UnifiedRequestCommandSerializer GetSerializer() => new Serializer(zone, consumeToken);
    }

    private sealed class Serializer : UnifiedRequestCommandSerializer
    {
        private Id<Zone> zone;
        private bool consumeToken;

        [UsedImplicitly] public Serializer() { }

        public Serializer(Zone zone, bool consumeToken)
        {
            this.zone = zone.Id;
            this.consumeToken = consumeToken;
        }

        protected override UnifiedRequestCommand GetSerialized(GameInstance game) =>
            new Implementation(game.State, game.State.ZoneLayer.FindZone(zone), consumeToken);

        public override void Serialize(INetBufferStream stream)
        {
            stream.Serialize(ref zone);
            stream.Serialize(ref consumeToken);
        }
    }
}
