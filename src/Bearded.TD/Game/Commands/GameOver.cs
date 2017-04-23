using Bearded.TD.Commands;
using Bearded.TD.Networking.Serialization;

namespace Bearded.TD.Game.Commands
{
    class GameOver : ICommand
    {
        public static ICommand Command(GameState game) => new GameOver(game);

        private readonly GameState game;

        private GameOver(GameState game)
        {
            this.game = game;
        }

        public void Execute() => game.Meta.DoGameOver();
        public ICommandSerializer Serializer { get; }
    }
}