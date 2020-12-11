using System;
using Bearded.TD.Commands;
using Bearded.TD.Game.Players;
using Bearded.TD.Networking.Serialization;
using Bearded.Utilities;
using JetBrains.Annotations;

namespace Bearded.TD.Game.Commands.General
{
    static class ChangePlayerState
    {
        public static IRequest<Player, GameInstance> Request(Player player, PlayerConnectionState state)
            => new Implementation(player, state);

        private sealed class Implementation : UnifiedRequestCommand
        {
            private readonly Player player;
            private readonly PlayerConnectionState state;

            public Implementation(Player player, PlayerConnectionState state)
            {
                this.player = player;
                this.state = state;
            }

            public override bool CheckPreconditions(Player actor)
            {
                if (actor != player) return false;

                switch (player.ConnectionState)
                {
                    case PlayerConnectionState.AwaitingLoadingData:
                    case PlayerConnectionState.FinishedLoading:
                    case PlayerConnectionState.Playing:
                        return false;
                    case PlayerConnectionState.Connecting:
                    case PlayerConnectionState.LoadingMods:
                    case PlayerConnectionState.Ready:
                        return state == PlayerConnectionState.Waiting;
                    case PlayerConnectionState.Waiting:
                        return state == PlayerConnectionState.Ready;
                    case PlayerConnectionState.ProcessingLoadingData:
                        return state == PlayerConnectionState.FinishedLoading;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }

            public override void Execute()
            {
                player.ConnectionState = state;
            }

            protected override UnifiedRequestCommandSerializer GetSerializer() => new Serializer(player, state);
        }

        private sealed class Serializer : UnifiedRequestCommandSerializer
        {
            private Id<Player> player;
            private byte state;

            [UsedImplicitly]
            public Serializer() {}

            public Serializer(Player player, PlayerConnectionState state)
            {
                this.player = player.Id;
                this.state = (byte) state;
            }

            protected override UnifiedRequestCommand GetSerialized(GameInstance game)
            {
                return new Implementation(game.PlayerFor(player), (PlayerConnectionState) state);
            }

            public override void Serialize(INetBufferStream stream)
            {
                stream.Serialize(ref player);
                stream.Serialize(ref state);
            }
        }
    }
}
