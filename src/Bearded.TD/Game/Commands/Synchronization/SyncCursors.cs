using System.Collections.Generic;
using System.Linq;
using Bearded.TD.Commands;
using Bearded.TD.Commands.Serialization;
using Bearded.TD.Game.Players;
using Bearded.TD.Networking.Serialization;
using Bearded.TD.Utilities;
using Bearded.Utilities;
using Bearded.Utilities.SpaceTime;
using JetBrains.Annotations;

namespace Bearded.TD.Game.Commands.Synchronization
{
    static class SyncCursors
    {
        public static IRequest<Player, GameInstance> Request(GameInstance game, Player player, Position2 cursorPosition)
            => new RequestImplementation(game, player, cursorPosition);

        public static ISerializableCommand<GameInstance> Command(
                GameInstance game, IDictionary<Player, Position2> cursorPositions)
            => new CommandImplementation(game, cursorPositions);

        private sealed class RequestImplementation : IRequest<Player, GameInstance>
        {
            private readonly GameInstance game;
            private readonly Player player;
            private readonly Position2 cursorPosition;

            public RequestImplementation(GameInstance game, Player player, Position2 cursorPosition)
            {
                this.game = game;
                this.player = player;
                this.cursorPosition = cursorPosition;
            }

            public bool CheckPreconditions(Player actor) => player == actor;

            public ISerializableCommand<GameInstance>? ToCommand()
            {
                // TODO: improve this
                // Instead of sending all the cursor events to clients one by one, we want to synchronize the cursors in
                // batch instead. The following is a bit of a hacky solution, but currently we don't have a good way of
                // executing commands on only on client. So right now we just execute this locally, and let the server
                // pick up this new position in the next sync round.
                game.PlayerCursors.SetPlayerCursorPosition(player, cursorPosition);
                return null;
            }

            public IRequestSerializer<Player, GameInstance> Serializer =>
                new Serializer(new Dictionary<Player, Position2> {{player, cursorPosition}});
        }

        private sealed class CommandImplementation : ISerializableCommand<GameInstance>
        {
            private readonly GameInstance game;
            private readonly IDictionary<Player, Position2> cursorPositions;

            public CommandImplementation(GameInstance game, IDictionary<Player, Position2> cursorPositions)
            {
                this.game = game;
                this.cursorPositions = cursorPositions;
            }

            public void Execute()
            {
                foreach (var (player, position) in cursorPositions)
                {
                    game.PlayerCursors.SetPlayerCursorPosition(player, position);
                }
            }

            public ICommandSerializer<GameInstance> Serializer => new Serializer(cursorPositions);
        }

        private sealed class Serializer : IRequestSerializer<Player, GameInstance>, ICommandSerializer<GameInstance>
        {
            private (Id<Player> player, Unit x, Unit y)[] cursorPositions = {};

            [UsedImplicitly]
            public Serializer() {}

            public Serializer(IDictionary<Player, Position2> cursorPositions)
            {
                this.cursorPositions =
                    cursorPositions.Select(pair => (pair.Key.Id, pair.Value.X, pair.Value.Y)).ToArray();
            }

            public IRequest<Player, GameInstance> GetRequest(GameInstance game)
            {
                DebugAssert.State.Satisfies(cursorPositions.Length == 1);

                var (player, x, y) = cursorPositions[0];
                return new RequestImplementation(game, game.PlayerFor(player), new Position2(x, y));
            }

            public ISerializableCommand<GameInstance> GetCommand(GameInstance game)
            {
                return new CommandImplementation(game, cursorPositions.ToDictionary(
                    tuple => game.PlayerFor(tuple.player),
                    tuple => new Position2(tuple.x, tuple.y)));
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
