using Bearded.TD.Commands;
using Bearded.TD.Commands.Serialization;
using Bearded.TD.Game.Simulation;
using Bearded.TD.Networking.Serialization;
using JetBrains.Annotations;

namespace Bearded.TD.Game.Loading;

static class CreateGameState
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
            game.InitializeState(new GameState(game.Meta, gameSettings));
        }

        ICommandSerializer<GameInstance> ISerializableCommand<GameInstance>.Serializer => new Serializer(gameSettings);
    }

    private sealed class Serializer : ICommandSerializer<GameInstance>
    {
        private readonly GameSettings.Serializer gameSettingsSerializer;

        [UsedImplicitly]
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