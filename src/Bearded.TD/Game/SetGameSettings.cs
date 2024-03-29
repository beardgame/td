using System.Linq;
using Bearded.TD.Commands;
using Bearded.TD.Commands.Serialization;
using Bearded.TD.Game.Meta;
using Bearded.TD.Game.Players;
using Bearded.TD.Networking.Serialization;
using Bearded.TD.Utilities.Collections;
using JetBrains.Annotations;

namespace Bearded.TD.Game;

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

            logModChanges();
        }

        private void logModChanges()
        {
            foreach (var enabledMod in gameSettings.ActiveModIds
                         .Where(id => game.Content.EnabledMods.All(m => m.Id != id))
                         .Select(game.Content.FindMod))
            {
                logSettingChange($"Mod enabled: {enabledMod.Name}");
            }

            foreach (var disabledMod in game.Content.EnabledMods
                         .Where(m => gameSettings.ActiveModIds.All(id => m.Id != id)))
            {
                logSettingChange($"Mod disabled: {disabledMod.Name}");
            }
        }

        private void logSettingChange(string message)
        {
            game.ChatLog.Add(new ChatMessage(null, message));
        }

        ICommandSerializer<GameInstance> ISerializableCommand<GameInstance>.Serializer => new Serializer(gameSettings);
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
