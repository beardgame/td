using System;
using Bearded.TD.Mods;
using Bearded.TD.Networking;
using Bearded.Utilities.IO;

namespace Bearded.TD.UI.Controls
{
    sealed class MainMenu
    {
        private readonly Logger logger;
        private readonly ContentManager contentManager;

        public MainMenu(Logger logger, ContentManager contentManager)
        {
            this.logger = logger;
            this.contentManager = contentManager;
        }

        public void OnHostGameButtonClicked()
        {
            var network = new ServerNetworkInterface();
            network.RegisterMessageHandler(new NetworkDebugMessageHandler(logger));
            logger.Info.Log("Click!");
            // Create lobby screen
        }

        public void OnJoinGameButtonClicked()
        {
            // Create connect to lobby screen
        }

        public void OnQuitGameButtonClicked() => throw new Exception("Goodbye");
    }
}
