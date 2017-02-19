using Bearded.Utilities;

namespace Bearded.TD.Game
{
    sealed class GameMeta
    {
        public Logger Logger { get; }
        public bool GameOver { get; private set; }

        public GameMeta(Logger logger) {
            Logger = logger;
        }

        public void DoGameOver()
        {
            GameOver = true;
        }
    }
}
