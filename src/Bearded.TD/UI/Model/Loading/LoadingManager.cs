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
            Logger = logger;
        }

        public virtual void Update(UpdateEventArgs args)
        {
            foreach (var msg in Network.GetMessages())
                if (msg.MessageType == NetIncomingMessageType.Data)
                    Game.DataMessageHandler.HandleIncomingMessage(msg);
        }

        public void IntegrateUI(InputManager inputManager)
        {
            var camera = new GameCamera();

            Game.IntegrateUI(camera);
        }
    }
}
