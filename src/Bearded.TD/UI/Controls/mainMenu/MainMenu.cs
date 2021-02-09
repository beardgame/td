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

        public void OnHostGameButtonClicked()
        {
            var network = new ServerNetworkInterface();
            network.RegisterMessageHandler(new NetworkDebugMessageHandler(logger));
            Navigation.Replace<Lobby, LobbyManager>(ServerLobbyManager.Create(network, logger, graphicsLoader, renderContext), this);
        }

        public void OnJoinGameButtonClicked() => Navigation.Replace<LobbyList>(this);

        public void OnQuitGameButtonClicked() => Navigation.Exit();
    }
}
