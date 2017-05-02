using amulware.Graphics;
using Lidgren.Network;

namespace Bearded.TD.Networking.Loading
{
    class LoadingManager
    {
        private readonly NetworkInterface networkInterface;
        private readonly IDataMessageHandler dataMessageHandler;

        public LoadingManager(NetworkInterface networkInterface, IDataMessageHandler dataMessageHandler)
        {
            this.networkInterface = networkInterface;
            this.dataMessageHandler = dataMessageHandler;
        }

        public void Update(UpdateEventArgs args)
        {
            foreach (var msg in networkInterface.GetMessages())
                if (msg.MessageType == NetIncomingMessageType.Data)
                    dataMessageHandler.HandleIncomingMessage(msg);
        }
    }
}
