using amulware.Graphics;
using Bearded.TD.Commands;
using Bearded.TD.Game;
using Bearded.TD.Game.Generation;
using Bearded.TD.Utilities.Input;
using Bearded.Utilities;
using Lidgren.Network;

namespace Bearded.TD.Networking.Loading
{
    class LoadingManager
    {
        protected IDispatcher Dispatcher { get; }
        private readonly NetworkInterface networkInterface;
        private readonly Logger logger;

        public GameInstance Game { get; }

        public LoadingManager(
            GameInstance game, IDispatcher dispatcher, NetworkInterface networkInterface, Logger logger)
        {
            Game = game;
            Dispatcher = dispatcher;
            this.networkInterface = networkInterface;
            this.logger = logger;
        }

        public virtual void Update(UpdateEventArgs args)
        {
            foreach (var msg in networkInterface.GetMessages())
                if (msg.MessageType == NetIncomingMessageType.Data)
                    Game.DataMessageHandler.HandleIncomingMessage(msg);
        }

        public void Debug_PopulateGame(InputManager inputManager)
        {
            var meta = new GameMeta(logger, Dispatcher, Game.Ids);
            var gameState = GameStateBuilder.Generate(meta, new DefaultTilemapGenerator(logger));
            var camera = new GameCamera(inputManager, meta, gameState.Level.Tilemap.Radius);

            Game.Debug_SetStateAndCamera(gameState, camera);
        }
    }
}
