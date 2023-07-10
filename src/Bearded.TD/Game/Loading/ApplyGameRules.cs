using Bearded.TD.Commands;
using Bearded.TD.Commands.Serialization;
using Bearded.TD.Content.Mods;
using Bearded.TD.Game.Simulation.Rules;
using Bearded.TD.Networking.Serialization;
using JetBrains.Annotations;

namespace Bearded.TD.Game.Loading;

static class ApplyGameRules
{
    public static ISerializableCommand<GameInstance> Command(
        GameInstance game, IGameModeBlueprint gameMode, int seed) =>
        new Implementation(game, gameMode, seed);

    private sealed class Implementation : ISerializableCommand<GameInstance>
    {
        private readonly GameInstance game;
        private readonly IGameModeBlueprint gameMode;
        private readonly int seed;

        public Implementation(GameInstance game, IGameModeBlueprint gameMode, int seed)
        {
            this.game = game;
            this.gameMode = gameMode;
            this.seed = seed;
        }

        public void Execute()
        {
            var context = new GameRuleContext(game.State, game.Meta.Events, game.SortedPlayers, game.Blueprints, seed);
            foreach (var ruleFactory in gameMode.Rules)
            {
                ruleFactory.Create().Execute(context);
            }
        }

        ICommandSerializer<GameInstance> ISerializableCommand<GameInstance>.Serializer =>
            new Serializer(gameMode, seed);
    }

    private sealed class Serializer : ICommandSerializer<GameInstance>
    {
        private ModAwareId gameModeId;
        private int seed;

        [UsedImplicitly]
        public Serializer() {}

        public Serializer(IGameModeBlueprint gameModeBlueprint, int seed)
        {
            gameModeId = gameModeBlueprint.Id;
            this.seed = seed;
        }

        public ISerializableCommand<GameInstance> GetCommand(GameInstance game) =>
            new Implementation(game, game.Blueprints.GameModes[gameModeId], seed);

        public void Serialize(INetBufferStream stream)
        {
            stream.Serialize(ref gameModeId);
            stream.Serialize(ref seed);
        }
    }
}
