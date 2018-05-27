using System;
using Bearded.TD.Mods;
using Bearded.TD.Networking;
using Bearded.TD.Screens;
using Bearded.TD.UI.Model.Lobby;
using Bearded.TD.UI.ViewModel;
using Bearded.Utilities.IO;

namespace Bearded.TD.UI.Model
{
    sealed class MainMenu : UIModel
    {
        private readonly MainMenuView view;
        private readonly Logger logger;
        private readonly ContentManager contentManager;

        public MainMenu(MainMenuView view, Logger logger, ContentManager contentManager)
            : base(view.Control)
        {
            this.view = view;
            this.logger = logger;
            this.contentManager = contentManager;

            view.HostGameClicked += onHostGameClicked;
            view.JoinGameClicked += onJoinGameClicked;
            view.QuitGameClicked += onQuitGameClicked;
        }

        private void onHostGameClicked()
        {
            var network = new ServerNetworkInterface();
            network.RegisterMessageHandler(new NetworkDebugMessageHandler(logger));
            // Create lobby screen
            Destroy();
        }

        private void onJoinGameClicked()
        {
            // Create connect to lobby screen
            Destroy();
        }

        private void onQuitGameClicked() => throw new Exception("Goodbye");
    }
}
