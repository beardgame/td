using System;
using Bearded.TD.Commands;
using Bearded.TD.Game.Players;
using Bearded.TD.Networking;
using Bearded.TD.Networking.Serialization;
using Bearded.TD.Utilities;

namespace Bearded.TD.Game.Commands
{
    static class ChangePlayerState
    {
        public static ICommand Command(Player player, PlayerConnectionState state)
            => new Implementation(player, state);
        public static IRequest Request(Player player, PlayerConnectionState state)
            => new Implementation(player, state);

        private class Implementation : UnifiedRequestCommand
        {
            private readonly Player player;
            private readonly PlayerConnectionState state;

            public Implementation(Player player, PlayerConnectionState state)
            {
                this.player = player;
                this.state = state;
            }

            public override bool CheckPreconditions()
            {
                switch (player.ConnectionState)
                {
                    case PlayerConnectionState.Connecting:
                    case PlayerConnectionState.AwaitingLoadingData:
                    case PlayerConnectionState.FinishedLoading:
                    case PlayerConnectionState.Playing:
                        return false;
                    case PlayerConnectionState.Waiting:
                        return state == PlayerConnectionState.Ready;
                    case PlayerConnectionState.Ready:
                        return state == PlayerConnectionState.Waiting;
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

        private class Serializer : UnifiedRequestCommandSerializer
        {
            private Id<Player> player;
            private byte state;

            // ReSharper disable once UnusedMember.Local
            public Serializer()
            {
            }

            public Serializer(Player player, PlayerConnectionState state)
            {
                this.player = player.Id;
                this.state = (byte) state;
            }

            protected override UnifiedRequestCommand GetSerialized(GameInstance game, Player sender)
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