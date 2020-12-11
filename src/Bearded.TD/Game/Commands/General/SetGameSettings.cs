using System.Linq;
using Bearded.TD.Commands;
using Bearded.TD.Commands.Serialization;
using Bearded.TD.Game.Meta;
using Bearded.TD.Game.Players;
using Bearded.TD.Networking.Serialization;
using Bearded.TD.Utilities.Collections;
using JetBrains.Annotations;

namespace Bearded.TD.Game.Commands.General
{
    static class SetGameSettings
    {
        public static ISerializableCommand<GameInstance> Command(GameInstance game, GameSettings gameSettings)
            => new Implementation(game, gameSettings);

        private sealed class Implementation : ISerializableCommand<GameInstance>
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
                logGameSettingChanges();
                game.SetGameSettings(gameSettings);
                game.Players.Where(p => p.ConnectionState == PlayerConnectionState.Ready)
                    .ForEach(p => p.ConnectionState = PlayerConnectionState.Waiting);
            }

            private void logGameSettingChanges()
            {
                if (game.GameSettings.LevelSize != gameSettings.LevelSize)
                {
                    logSettingChange($"Level size changed: {gameSettings.LevelSize}");
                }
                if (game.GameSettings.LevelGenerationMethod != gameSettings.LevelGenerationMethod)
                {
                    logSettingChange($"Level generation changed: {gameSettings.LevelGenerationMethod}");
                }
                if (game.GameSettings.WorkerDistributionMethod != gameSettings.WorkerDistributionMethod)
                {
                    logSettingChange($"Worker distribution changed: {gameSettings.WorkerDistributionMethod}");
                }
            }

            private void logSettingChange(string message)
            {
                game.ChatLog.Add(new ChatMessage(null, message));
            }

            public ICommandSerializer<GameInstance> Serializer => new Serializer(gameSettings);
        }

        private sealed class Serializer : ICommandSerializer<GameInstance>
        {
            private readonly GameSettings.Serializer gameSettingsSerializer;

            [UsedImplicitly]
            public Serializer() : this(new GameSettings.Builder().Build()) { }

            public Serializer(IGameSettings gameSettings)
            {
                gameSettingsSerializer = new GameSettings.Serializer(gameSettings);
            }

            public ISerializableCommand<GameInstance> GetCommand(GameInstance game)
                => new Implementation(game, gameSettingsSerializer.ToGameSettings());

            public void Serialize(INetBufferStream stream) => gameSettingsSerializer.Serialize(stream);
        }
    }
}
