using Bearded.TD.Commands;
using Bearded.TD.Game.Synchronization;
using Bearded.Utilities;
using Bearded.Utilities.IO;

namespace Bearded.TD.Game
{
    sealed class GameMeta
    {
        public IDispatcher<GameInstance> Dispatcher { get; }
        public IGameSynchronizer Synchronizer { get; }
        public IdManager Ids { get; }
        public Logger Logger { get; }
        public bool GameOver { get; private set; }
        public GameEvents Events { get; } = new GameEvents();

        public GameMeta(Logger logger, IDispatcher<GameInstance> dispatcher, IGameSynchronizer synchronizer, IdManager ids)
        {
            Logger = logger;
            Synchronizer = synchronizer;
            Dispatcher = dispatcher;
            Ids = ids;
        }

        public void DoGameOver()
        {
            GameOver = true;
        }
    }
}
