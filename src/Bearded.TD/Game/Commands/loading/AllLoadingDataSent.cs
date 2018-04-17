﻿using Bearded.TD.Commands;
using Bearded.TD.Networking;
using Bearded.TD.Networking.Serialization;
using Bearded.TD.Utilities.Collections;

namespace Bearded.TD.Game.Commands
{
    static class AllLoadingDataSent
    {
        public static ISerializableCommand<GameInstance> Command(GameInstance game)
            => new Implementation(game);

        private class Implementation : ISerializableCommand<GameInstance>
        {
            private readonly GameInstance game;

            public Implementation(GameInstance game)
            {
                this.game = game;
            }

            public void Execute()
            {
                game.State.FinishLoading();
                game.Players.ForEach(p => p.ConnectionState = PlayerConnectionState.ProcessingLoadingData);
            }

            public ICommandSerializer<GameInstance> Serializer => new Serializer();
        }

        private class Serializer : ICommandSerializer<GameInstance>
        {
            // ReSharper disable once UnusedMember.Local
            public Serializer()
            {
            }

            public ISerializableCommand<GameInstance> GetCommand(GameInstance game)
                => new Implementation(game);

            public void Serialize(INetBufferStream stream)
            {
            }
        }
    }
}
