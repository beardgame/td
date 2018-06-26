using Bearded.TD.Meta;
using Bearded.TD.Networking;
using Bearded.UI.Navigation;
using Bearded.Utilities;
using Bearded.Utilities.IO;
using Lidgren.Network;

namespace Bearded.TD.UI.Controls
{
    sealed class LobbyList : NavigationNode<Void>, INetworkMessageHandler
    {
        private ClientNetworkInterface networkInterface;

        protected override void Initialize(DependencyResolver dependencies, Void parameters)
        {
            networkInterface = new ClientNetworkInterface();
            networkInterface.RegisterMessageHandler(new NetworkDebugMessageHandler(dependencies.Resolve<Logger>()));	
            networkInterface.RegisterMessageHandler(this);
        }

        public override void Terminate()
        {
            base.Terminate();

            networkInterface.UnregisterMessageHandler(this);
        }

        public bool Accepts(NetIncomingMessage message)
            => message.MessageType == NetIncomingMessageType.StatusChanged
                || message.MessageType == NetIncomingMessageType.UnconnectedData
                || message.MessageType == NetIncomingMessageType.NatIntroductionSuccess;

        public void Handle(NetIncomingMessage message)
        {
            // TODO
        }

        public void OnConnectManualButtonClicked(string host)
        {
            networkInterface.Connect(host, new ClientInfo(playerName));
        }

        public void OnBackToMenuButtonClicked()
        {
            networkInterface.Shutdown();
            Navigation.Replace<MainMenu>(this);
        }

        private static string playerName
            => UserSettings.Instance.Misc.Username?.Length > 0	
                ? UserSettings.Instance.Misc.Username	
                : "player";
    }
}
