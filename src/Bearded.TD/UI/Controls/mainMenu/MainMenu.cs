using Bearded.TD.Content;
using Bearded.TD.Networking;
using Bearded.UI.Navigation;
using Bearded.Utilities.IO;
using Void = Bearded.Utilities.Void;

namespace Bearded.TD.UI.Controls
{
    sealed class MainMenu : NavigationNode<Void>
    {
        private Logger logger;
        private IGraphicsLoader graphicsLoader;

        protected override void Initialize(DependencyResolver dependencies, Void _)
        {
            logger = dependencies.Resolve<Logger>();
            graphicsLoader = dependencies.Resolve<IGraphicsLoader>();
        }

        public void OnHostGameButtonClicked()
        {
            var network = new ServerNetworkInterface();
            network.RegisterMessageHandler(new NetworkDebugMessageHandler(logger));
            Navigation.Replace<Lobby, LobbyManager>(ServerLobbyManager.Create(network, logger, graphicsLoader), this);
        }

        public void OnJoinGameButtonClicked() => Navigation.Replace<LobbyList>(this);

        public void OnQuitGameButtonClicked() => Navigation.Exit();
    }
}
