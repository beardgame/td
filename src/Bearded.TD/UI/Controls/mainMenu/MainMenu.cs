using System;
using Bearded.TD.Content;
using Bearded.TD.Networking;
using Bearded.TD.Rendering;
using Bearded.UI.Navigation;
using Bearded.Utilities.IO;
using Void = Bearded.Utilities.Void;

namespace Bearded.TD.UI.Controls
{
    sealed class MainMenu : NavigationNode<Void>
    {
        private Logger logger = null!;
        private IGraphicsLoader graphicsLoader = null!;
        private RenderContext renderContext;

        protected override void Initialize(DependencyResolver dependencies, Void _)
        {
            logger = dependencies.Resolve<Logger>();
            graphicsLoader = dependencies.Resolve<IGraphicsLoader>();
            renderContext = dependencies.Resolve<RenderContext>();
        }

        public void OnQuickGameButtonClicked()
        {
            startLobby(ServerLobbyManager.CreateWithReadyPlayer);
        }

        public void OnHostGameButtonClicked()
        {
            startLobby(ServerLobbyManager.Create);
        }

        private void startLobby(
            Func<ServerNetworkInterface, Logger, IGraphicsLoader,
            RenderContext, ServerLobbyManager> lobbyManagerFactory
            )
        {
            var network = new ServerNetworkInterface();
            network.RegisterMessageHandler(new NetworkDebugMessageHandler(logger));
            var lobbyManager = lobbyManagerFactory(network, logger, graphicsLoader, renderContext);
            Navigation.Replace<Lobby, LobbyManager>(lobbyManager, this);
        }

        public void OnJoinGameButtonClicked() => Navigation.Replace<LobbyList>(this);

        public void OnQuitGameButtonClicked() => Navigation.Exit();
    }
}
