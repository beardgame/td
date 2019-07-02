using amulware.Graphics;
using Bearded.TD.Content;
using Bearded.TD.Networking;
using Bearded.UI.Navigation;
using Bearded.Utilities;
using Bearded.Utilities.IO;
using Void = Bearded.Utilities.Void;

namespace Bearded.TD.UI.Controls
{
    sealed class MainMenu : UpdateableNavigationNode<Void>
    {
        private Logger logger;
        private ContentManager contentManager;

        private bool isActivated;
        public event VoidEventHandler Activated;

        protected override void Initialize(DependencyResolver dependencies, Void _)
        {
            logger = dependencies.Resolve<Logger>();
            contentManager = dependencies.Resolve<ContentManager>();

            dependencies.Resolve<UIUpdater>().Add(this);
        }

        public override void Update(UpdateEventArgs args)
        {
            if (isActivated) return;
            Activated?.Invoke();
            isActivated = true;
        }

        public void OnHostGameButtonClicked()
        {
            var network = new ServerNetworkInterface();
            network.RegisterMessageHandler(new NetworkDebugMessageHandler(logger));
            Navigation.Replace<Lobby, LobbyManager>(ServerLobbyManager.Create(network, logger, contentManager), this);
        }

        public void OnJoinGameButtonClicked() => Navigation.Replace<LobbyList>(this);

        public void OnQuitGameButtonClicked() => Navigation.Exit();
    }
}
