using Bearded.TD.Commands;
using Bearded.Utilities;

namespace Bearded.TD.Game
{
    sealed class GameMeta
    {
        public IDispatcher Dispatcher { get; }
        public Logger Logger { get; }
        public bool GameOver { get; private set; }

        public GameMeta(Logger logger, IDispatcher dispatcher)
        {
            Logger = logger;
            Dispatcher = dispatcher;
        }

        public void DoGameOver()
        {
            GameOver = true;
        }
    }
}
