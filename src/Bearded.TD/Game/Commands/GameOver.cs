using Bearded.TD.Commands;

namespace Bearded.TD.Game.Commands
{
    class GameOver : ICommand
    {
        public static ICommand Command(GameObject obj) => new GameOver(obj.Game);

        private readonly GameState game;

        private GameOver(GameState game)
        {
            this.game = game;
        }

        public void Execute() => game.Meta.DoGameOver();
    }
}