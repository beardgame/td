﻿using Bearded.TD.Commands;
using Bearded.TD.Commands.Serialization;
using Bearded.TD.Networking.Serialization;

namespace Bearded.TD.Game.Commands
{
    static class CreateGameState
    {
        public static ISerializableCommand<GameInstance> Command(GameInstance game, GameSettings gameSettings)
            => new Implementation(game, gameSettings);

        private class Implementation : ISerializableCommand<GameInstance>
        {
            private readonly GameInstance game;
            private readonly GameSettings gameSettings;

            public Implementation(GameInstance game, GameSettings gameSettings)
            {
                this.game = game;
                this.gameSettings = gameSettings;
            }

            public void Execute()
            {
                game.InitialiseState(new GameState.GameState(game.Meta, gameSettings));
            }

            public ICommandSerializer<GameInstance> Serializer => new Serializer(gameSettings);
        }

        private class Serializer : ICommandSerializer<GameInstance>
        {
            private readonly GameSettings.Serializer gameSettingsSerializer;

            // ReSharper disable once UnusedMember.Local
            public Serializer() : this(new GameSettings.Builder().Build()) { }

            public Serializer(GameSettings gameSettings)
            {
                gameSettingsSerializer = new GameSettings.Serializer(gameSettings);
            }

            public ISerializableCommand<GameInstance> GetCommand(GameInstance game)
                => new Implementation(game, gameSettingsSerializer.ToGameSettings());

            public void Serialize(INetBufferStream stream) => gameSettingsSerializer.Serialize(stream);
        }
    }
}
