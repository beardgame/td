using Bearded.TD.Commands;
using Bearded.TD.Commands.Serialization;
using Bearded.TD.Content.Mods;
using Bearded.TD.Game.GameState.Rules;
using Bearded.TD.Networking.Serialization;

namespace Bearded.TD.Game.Commands
{
    static class ApplyGameRules
    {
        public static ISerializableCommand<GameInstance> Command(GameInstance game, IGameModeBlueprint gameMode)
            => new Implementation(game, gameMode);

        private class Implementation : ISerializableCommand<GameInstance>
        {
            private readonly GameInstance game;
            private readonly IGameModeBlueprint gameMode;

            public Implementation(GameInstance game, IGameModeBlueprint gameMode)
            {
                this.game = game;
                this.gameMode = gameMode;
            }

            public void Execute()
            {
                foreach (var ruleFactory in gameMode.Rules)
                {
                    ruleFactory.Create().OnAdded(game.State, game.Meta.Events);
                }
            }

            public ICommandSerializer<GameInstance> Serializer => new Serializer(gameMode);
        }

        private class Serializer : ICommandSerializer<GameInstance>
        {
            private ModAwareId gameModeId;

            public Serializer(IGameModeBlueprint gameModeBlueprint)
            {
                gameModeId = gameModeBlueprint.Id;
            }

            // ReSharper disable once UnusedMember.Local
            public Serializer() {}

            public ISerializableCommand<GameInstance> GetCommand(GameInstance game) =>
                new Implementation(game, game.Blueprints.GameModes[gameModeId]);

            public void Serialize(INetBufferStream stream)
            {
                stream.Serialize(ref gameModeId);
            }
        }
    }
}
