using Bearded.TD.Mods;
using Bearded.TD.Networking;
using Bearded.UI.Navigation;
using Bearded.Utilities.IO;
using Void = Bearded.Utilities.Void;

namespace Bearded.TD.UI.Controls
{
    sealed class MainMenu : NavigationNode<Void>
    {
        private Logger logger;
        private ContentManager contentManager;

        protected override void Initialize(DependencyResolver dependencies, Void _)
        {
            logger = dependencies.Resolve<Logger>();
            contentManager = dependencies.Resolve<ContentManager>();
        }

        public void OnHostGameButtonClicked()
        {
            var network = new ServerNetworkInterface();
            network.RegisterMessageHandler(new NetworkDebugMessageHandler(logger));
            Navigation.GoTo<Lobby, LobbyManager>(new ServerLobbyManager(network, logger, contentManager));
        }

        public void OnJoinGameButtonClicked() => Navigation.GoTo<LobbyList>();

        public void OnQuitGameButtonClicked() => Navigation.Exit();
    }
}
