using Bearded.TD.Commands;
using Bearded.TD.Commands.Serialization;
using Bearded.TD.Content.Mods;
using Bearded.TD.Game.Simulation.Rules;
using Bearded.TD.Networking.Serialization;
using JetBrains.Annotations;

namespace Bearded.TD.Game.Commands.Loading
{
    static class ApplyGameRules
    {
        public static ISerializableCommand<GameInstance> Command(GameInstance game, IGameModeBlueprint gameMode)
            => new Implementation(game, gameMode);

        private sealed class Implementation : ISerializableCommand<GameInstance>
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
                var context = new GameRuleContext(game.State, game.Meta.Events);
                foreach (var ruleFactory in gameMode.Rules)
                {
                    ruleFactory.Create().Initialize(context);
                }
            }

            public ICommandSerializer<GameInstance> Serializer => new Serializer(gameMode);
        }

        private sealed class Serializer : ICommandSerializer<GameInstance>
        {
            private ModAwareId gameModeId;

            [UsedImplicitly]
            public Serializer() {}

            public Serializer(IGameModeBlueprint gameModeBlueprint)
            {
                gameModeId = gameModeBlueprint.Id;
            }

            public ISerializableCommand<GameInstance> GetCommand(GameInstance game) =>
                new Implementation(game, game.Blueprints.GameModes[gameModeId]);

            public void Serialize(INetBufferStream stream)
            {
                stream.Serialize(ref gameModeId);
            }
        }
    }
}
