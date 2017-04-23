using Bearded.TD.Commands;
using Bearded.TD.Utilities;
using Bearded.Utilities;

namespace Bearded.TD.Game
{
    sealed class GameMeta
    {
        public IDispatcher Dispatcher { get; }
        public IdManager Ids { get; }
        public Logger Logger { get; }
        public bool GameOver { get; private set; }

        public GameMeta(Logger logger, IDispatcher dispatcher, IdManager ids)
        {
            Logger = logger;
            Dispatcher = dispatcher;
            Ids = ids;
        }

        public void DoGameOver()
        {
            GameOver = true;
        }
    }
}
