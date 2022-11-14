using Bearded.TD.Commands;
using Bearded.TD.Content.Mods;
using Bearded.TD.Game.Commands;
using Bearded.TD.Game.Players;
using Bearded.TD.Game.Simulation.Factions;
using Bearded.TD.Game.Simulation.GameObjects;
using Bearded.TD.Networking.Serialization;
using Bearded.Utilities;
using Bearded.Utilities.SpaceTime;
using JetBrains.Annotations;

namespace Bearded.TD.Game.Meta;

static class Ping
{
    public static IRequest<Player, GameInstance> Request(GameInstance game, Player player, Position2 position)
    {
        var ping = new Implementation(game, player, position);
        ping.ExecuteLocally();
        return ping;
    }

    private sealed class Implementation : UnifiedRequestCommand
    {
        private GameInstance game { get; }
        private Player player { get; }
        private Position2 position { get; }

        public Implementation(GameInstance game, Player player, Position2 position)
        {
            this.game = game;
            this.player = player;
            this.position = position;
        }

        public override bool CheckPreconditions(Player actor)
            => player == actor;

        public override void Execute()
        {
            if (player.Id != game.Me.Id)
                execute();
        }

        public void ExecuteLocally() => execute();

        private void execute()
        {
            var blueprint = game.Blueprints.GameObjects[new ModAwareId("default", "player-ping")];
            var ping = GameObjectFactory.CreateFromBlueprintWithDefaultRenderer(blueprint, null, position.WithZ(0));
            ping.AddComponent(new FactionProvider(player.Faction));
            game.State.Add(ping);
        }

        protected override UnifiedRequestCommandSerializer GetSerializer()
            => new Serializer(player, position);
    }

    private sealed class Serializer : UnifiedRequestCommandSerializer
    {
        private Id<Player> player;
        private Position2 position;

        public Serializer(Player player, Position2 position)
        {
            this.player = player.Id;
            this.position = position;
        }

        [UsedImplicitly]
        public Serializer() {}

        protected override UnifiedRequestCommand GetSerialized(GameInstance game)
            => new Implementation(game, game.PlayerFor(player), position);

        public override void Serialize(INetBufferStream stream)
        {
            stream.Serialize(ref player);
            stream.Serialize(ref position);
        }
    }
}
