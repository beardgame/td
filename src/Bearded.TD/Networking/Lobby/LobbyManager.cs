using System.Collections.Generic;
using amulware.Graphics;
using Bearded.TD.Commands;
using Bearded.TD.Game;
using Bearded.TD.Game.Generation;
using Bearded.TD.Game.Players;
using Bearded.TD.Utilities.Input;
using Bearded.Utilities;

namespace Bearded.TD.Networking.Lobby
{
    abstract class LobbyManager
    {
        protected Logger Logger;

        public abstract bool GameStarted { get; }
        public abstract IReadOnlyList<LobbyPlayer> Players { get; }

        protected LobbyManager(Logger logger)
        {
            Logger = logger;
        }

        public abstract void Update(UpdateEventArgs args);

        public abstract void ToggleReadyState();

        public abstract GameInstance BuildInstance(InputManager inputManager);

        protected GameInstance BuildInstance(
            Player player,
            IRequestDispatcher requestDispatcher,
            IDispatcher dispatcher,
            InputManager inputManager)
        {
            var meta = new GameMeta(Logger, dispatcher);

            var gameState = GameStateBuilder.Generate(meta, new DefaultTilemapGenerator(Logger));
            return new GameInstance(
                player,
                gameState,
                new GameCamera(inputManager, meta, gameState.Level.Tilemap.Radius),
                requestDispatcher
                );
        }
    }
}