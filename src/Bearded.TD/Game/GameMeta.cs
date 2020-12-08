﻿using System;
using Bearded.TD.Commands;
using Bearded.TD.Game.GameState.Events;
using Bearded.TD.Game.GameState.GameLoop;
using Bearded.TD.Game.Synchronization;
using Bearded.Utilities;
using Bearded.Utilities.IO;

namespace Bearded.TD.Game
{
    sealed class GameMeta
    {
        private Blueprints? blueprints;

        public IDispatcher<GameInstance> Dispatcher { get; }
        public IGameSynchronizer Synchronizer { get; }
        public IdManager Ids { get; }
        public Logger Logger { get; }
        public bool GameOver { get; private set; }
        public GlobalGameEvents Events { get; } = new GlobalGameEvents();
        public Blueprints Blueprints => blueprints!;

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

            if (this.blueprints != null)
                throw new InvalidOperationException("Can only set blueprints once.");

            this.blueprints = blueprints;
        }

        public void DoGameOver()
        {
            GameOver = true;
            Events.Send(new GameOverTriggered());
        }
    }
}
