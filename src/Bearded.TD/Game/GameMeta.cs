using System;
using Bearded.TD.Commands;
using Bearded.TD.Game.Events;
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
        public Blueprints Blueprints { get; private set; }

        public GameMeta(Logger logger, IDispatcher<GameInstance> dispatcher, IGameSynchronizer synchronizer, IdManager ids)
        {
            Logger = logger;
            Synchronizer = synchronizer;
            Dispatcher = dispatcher;
            Ids = ids;
        }

        public void SetBlueprints(Blueprints blueprints)
        {
            // TODO: remove this mutability.
            // Might have to initialise the Meta class late in the GameInstance,
            // but that will require some refactoring

            if (Blueprints != null)
                throw new InvalidOperationException("Can only set blueprints once.");

            Blueprints = blueprints;
        }

        public void DoGameOver()
        {
            GameOver = true;
            Events.Send(new GameOverTriggered());
        }
    }
}
