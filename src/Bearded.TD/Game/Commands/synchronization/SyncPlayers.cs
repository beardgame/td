﻿using System.Collections.Generic;
using System.Linq;
using Bearded.TD.Commands;
using Bearded.TD.Game.Players;
using Bearded.TD.Networking.Serialization;
using Bearded.Utilities;

namespace Bearded.TD.Game.Commands
{
    static class SyncPlayers
    {
        public static ICommand Command(GameInstance game)
            => new Implementation(game.Players.Select(p => (p, p.GetCurrentState())).ToList());

        private class Implementation : ICommand
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

            public ICommandSerializer Serializer => new Serializer(players);
        }

        private class Serializer : ICommandSerializer
        {
            private (Id<Player> id, PlayerState state)[] players;

            // ReSharper disable once UnusedMember.Local
            public Serializer() { }

            public Serializer(IEnumerable<(Player player, PlayerState state)> players)
            {
                this.players = players.Select(tuple => (tuple.player.Id, tuple.state)).ToArray();
            }

            public ICommand GetCommand(GameInstance game)
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
