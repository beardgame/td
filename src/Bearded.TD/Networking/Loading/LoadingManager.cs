using amulware.Graphics;
using Bearded.TD.Commands;
using Bearded.TD.Game;
using Bearded.TD.Utilities.Input;
using Bearded.Utilities;
using Lidgren.Network;

namespace Bearded.TD.Networking.Loading
{
    class LoadingManager
    {
        protected IDispatcher Dispatcher { get; }
        public NetworkInterface Network { get; }
        protected Logger Logger { get; }

        public GameInstance Game { get; }

        public LoadingManager(
            GameInstance game, IDispatcher dispatcher, NetworkInterface networkInterface, Logger logger)
        {
            Game = game;
            Dispatcher = dispatcher;
            Network = networkInterface;
            this.Logger = logger;
        }

        public virtual void Update(UpdateEventArgs args)
        {
            foreach (var msg in Network.GetMessages())
                if (msg.MessageType == NetIncomingMessageType.Data)
                    Game.DataMessageHandler.HandleIncomingMessage(msg);
        }

        public void IntegrateUI(InputManager inputManager)
        {
            //var gameState = GameStateBuilder.Generate(meta, new DefaultTilemapGenerator(logger));

            var camera = new GameCamera(inputManager, Game.State.Meta, Game.State.Level.Tilemap.Radius);

            Game.IntegrateUI(camera);
        }
    }
}
