using System.Collections.Generic;
using System.Linq;
using Bearded.TD.Commands;
using Bearded.TD.Commands.Serialization;
using Bearded.TD.Content.Mods;
using Bearded.TD.Game.Players;
using Bearded.TD.Game.Simulation.Components;
using Bearded.TD.Networking.Serialization;
using Bearded.TD.Utilities;
using Bearded.Utilities;
using Bearded.Utilities.SpaceTime;
using JetBrains.Annotations;

namespace Bearded.TD.Game.Commands.Synchronization
{
    static class SyncCursors
    {
        public static IRequest<Player, GameInstance> Request(
                GameInstance game, Player player, Position2 cursorPosition, IComponentOwnerBlueprint? attachedGhost) =>
            new RequestImplementation(game, player, cursorPosition, attachedGhost);

        public static ISerializableCommand<GameInstance> Command(
                GameInstance game, IDictionary<Player, (Position2, IComponentOwnerBlueprint?)> cursorStates) =>
            new CommandImplementation(game, cursorStates);

        private sealed class RequestImplementation : IRequest<Player, GameInstance>
        {
            private readonly GameInstance game;
            private readonly Player player;
            private readonly Position2 cursorPosition;
            private readonly IComponentOwnerBlueprint? attachedGhost;

            public RequestImplementation(
                GameInstance game, Player player, Position2 cursorPosition, IComponentOwnerBlueprint? attachedGhost)
            {
                this.game = game;
                this.player = player;
                this.cursorPosition = cursorPosition;
                this.attachedGhost = attachedGhost;
            }

            public bool CheckPreconditions(Player actor) => player == actor;

            public ISerializableCommand<GameInstance>? ToCommand()
            {
                // TODO: improve this
                // Instead of sending all the cursor events to clients one by one, we want to synchronize the cursors in
                // batch instead. The following is a bit of a hacky solution, but currently we don't have a good way of
                // executing commands on only on client. So right now we just execute this locally, and let the server
                // pick up this new position in the next sync round.
                game.PlayerCursors.SyncPlayerCursorPosition(player, cursorPosition, attachedGhost);
                return null;
            }

            IRequestSerializer<Player, GameInstance> IRequest<Player, GameInstance>.Serializer =>
                new Serializer(new Dictionary<Player, (Position2, IComponentOwnerBlueprint?)>
                {
                    { player, (cursorPosition, attachedGhost) }
                });
        }

        private sealed class CommandImplementation : ISerializableCommand<GameInstance>
        {
            private readonly GameInstance game;
            private readonly IDictionary<Player, (Position2, IComponentOwnerBlueprint?)> cursorStates;

            public CommandImplementation(
                GameInstance game, IDictionary<Player, (Position2, IComponentOwnerBlueprint?)> cursorStates)
            {
                this.game = game;
                this.cursorStates = cursorStates;
            }

            public void Execute()
            {
                foreach (var (player, (position, blueprint)) in cursorStates)
                {
                    game.PlayerCursors.SyncPlayerCursorPosition(player, position, blueprint);
                }
            }

            ICommandSerializer<GameInstance> ISerializableCommand<GameInstance>.Serializer =>
                new Serializer(cursorStates);
        }

        private sealed class Serializer : IRequestSerializer<Player, GameInstance>, ICommandSerializer<GameInstance>
        {
            private (Id<Player> Player, Unit X, Unit Y, ModAwareId AttachedGhost)[] cursorStates = {};

            [UsedImplicitly]
            public Serializer() {}

            public Serializer(
                IDictionary<Player, (Position2 Position, IComponentOwnerBlueprint? AttachedGhost)> cursorStates)
            {
                this.cursorStates = cursorStates.Select(pair => (
                    pair.Key.Id,
                    pair.Value.Position.X,
                    pair.Value.Position.Y,
                    pair.Value.AttachedGhost?.Id ?? ModAwareId.Invalid)
                ).ToArray();
            }

            public IRequest<Player, GameInstance> GetRequest(GameInstance game)
            {
                DebugAssert.State.Satisfies(cursorStates.Length == 1);

                var (player, x, y, ghost) = cursorStates[0];
                return new RequestImplementation(
                    game,
                    game.PlayerFor(player),
                    new Position2(x, y),
                    ghost.IsValid ? game.Blueprints.ComponentOwners[ghost] : null);
            }

            public ISerializableCommand<GameInstance> GetCommand(GameInstance game)
            {
                return new CommandImplementation(
                    game,
                    cursorStates.ToDictionary(
                        tuple => game.PlayerFor(tuple.Player),
                        tuple => (
                            new Position2(tuple.X, tuple.Y),
                            tuple.AttachedGhost.IsValid
                                ? game.Blueprints.ComponentOwners[tuple.AttachedGhost]
                                : null)));
            }

            public void Serialize(INetBufferStream stream)
            {
                stream.SerializeArrayCount(ref cursorStates);
                for (var i = 0; i < cursorStates.Length; i++)
                {
                    stream.Serialize(ref cursorStates[i].Player);
                    stream.Serialize(ref cursorStates[i].X);
                    stream.Serialize(ref cursorStates[i].Y);
                    stream.Serialize(ref cursorStates[i].AttachedGhost);
                }
            }
        }
    }
}
