using System.Collections.Generic;
using System.Linq;
using Bearded.TD.Commands;
using Bearded.TD.Commands.Serialization;
using Bearded.TD.Game.Players;
using Bearded.TD.Networking.Serialization;
using Bearded.Utilities;
using JetBrains.Annotations;

namespace Bearded.TD.Game.Commands.Synchronization
{
    static class SyncPlayers
    {
        public static ISerializableCommand<GameInstance> Command(GameInstance game)
            => new Implementation(game.Players.Select(p => (p, p.GetCurrentState())).ToList());

        private sealed class Implementation : ISerializableCommand<GameInstance>
        {
            private readonly IList<(Player, PlayerState)> players;

            public Implementation(IList<(Player, PlayerState)> players)
            {
                this.players = players;
            }

            public void Execute()
            {
                foreach (var (player, state) in players)
                {
                    player.SyncFrom(state);
                }
            }

            ICommandSerializer<GameInstance> ISerializableCommand<GameInstance>.Serializer => new Serializer(players);
        }

        private sealed class Serializer : ICommandSerializer<GameInstance>
        {
            private (Id<Player> id, PlayerState state)[] players = {};

            [UsedImplicitly]
            public Serializer() { }

            public Serializer(IEnumerable<(Player player, PlayerState state)> players)
            {
                this.players = players.Select(tuple => (tuple.player.Id, tuple.state)).ToArray();
            }

            public ISerializableCommand<GameInstance> GetCommand(GameInstance game)
                => new Implementation(players.Select(tuple => (game.PlayerFor(tuple.id), tuple.state)).ToList());

            public void Serialize(INetBufferStream stream)
            {
                stream.SerializeArrayCount(ref players);
                for (var i = 0; i < players.Length; i++)
                {
                    stream.Serialize(ref players[i].id);
                    stream.Serialize(ref players[i].state);
                }
            }
        }
    }
}
