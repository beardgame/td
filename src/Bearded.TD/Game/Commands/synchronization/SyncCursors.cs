using System.Collections.Generic;
using System.Linq;
using Bearded.TD.Commands;
using Bearded.TD.Commands.Serialization;
using Bearded.TD.Game.Players;
using Bearded.TD.Networking.Serialization;
using Bearded.TD.Tiles;
using Bearded.TD.Utilities;
using Bearded.Utilities;

namespace Bearded.TD.Game.Commands
{
    static class SyncCursors
    {
        public static IRequest<GameInstance> Request(GameInstance game, Player player, Tile cursorPosition)
            => new RequestImplementation(game, player, cursorPosition);

        public static ISerializableCommand<GameInstance> Command(
                GameInstance game, IDictionary<Player, Tile> cursorPositions)
            => new CommandImplementation(game, cursorPositions);

        private class RequestImplementation : IRequest<GameInstance>
        {
            private readonly GameInstance game;
            private readonly Player player;
            private readonly Tile cursorPosition;

            public RequestImplementation(GameInstance game, Player player, Tile cursorPosition)
            {
                this.game = game;
                this.player = player;
                this.cursorPosition = cursorPosition;
            }

            public bool CheckPreconditions() => true;

            public ISerializableCommand<GameInstance> ToCommand()
            {
                game.PlayerCursors.SetPlayerCursorPosition(player, cursorPosition);
                return null;
            }

            public IRequestSerializer<GameInstance> Serializer =>
                new Serializer(new Dictionary<Player, Tile> {{player, cursorPosition}});
        }

        private class CommandImplementation : ISerializableCommand<GameInstance>
        {
            private readonly GameInstance game;
            private readonly IDictionary<Player, Tile> cursorPositions;

            public CommandImplementation(GameInstance game, IDictionary<Player, Tile> cursorPositions)
            {
                this.game = game;
                this.cursorPositions = cursorPositions;
            }

            public void Execute()
            {
                foreach (var (player, tile) in cursorPositions)
                {
                    game.PlayerCursors.SetPlayerCursorPosition(player, tile);
                }
            }

            public ICommandSerializer<GameInstance> Serializer => new Serializer(cursorPositions);
        }

        private class Serializer : IRequestSerializer<GameInstance>, ICommandSerializer<GameInstance>
        {
            private (Id<Player> player, int x, int y)[] cursorPositions;

            // ReSharper disable once UnusedMember.Local
            public Serializer() {}

            public Serializer(IDictionary<Player, Tile> cursorPositions)
            {
                this.cursorPositions =
                    cursorPositions.Select(pair => (pair.Key.Id, pair.Value.X, pair.Value.Y)).ToArray();
            }

            public IRequest<GameInstance> GetRequest(GameInstance game)
            {
                DebugAssert.State.Satisfies(cursorPositions.Length == 1);

                var (player, x, y) = cursorPositions[0];
                return new RequestImplementation(game, game.PlayerFor(player), new Tile(x, y));
            }

            public ISerializableCommand<GameInstance> GetCommand(GameInstance game)
            {
                return new CommandImplementation(game, cursorPositions.ToDictionary(
                    tuple => game.PlayerFor(tuple.player),
                    tuple => new Tile(tuple.x, tuple.y)));
            }

            public void Serialize(INetBufferStream stream)
            {
                stream.SerializeArrayCount(ref cursorPositions);
                for (var i = 0; i < cursorPositions.Length; i++)
                {
                    stream.Serialize(ref cursorPositions[i].player);
                    stream.Serialize(ref cursorPositions[i].x);
                    stream.Serialize(ref cursorPositions[i].y);
                }
            }
        }
    }
}
