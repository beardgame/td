using System;
using Bearded.Graphics;
using Bearded.TD.Content;
using Bearded.TD.Networking;
using Bearded.TD.Rendering;
using Bearded.UI.Navigation;
using Bearded.Utilities.IO;

namespace Bearded.TD.UI.Controls;

sealed class MainMenu : NavigationNode<Intent>
{
    private Logger logger = null!;
    private ContentManager contentManager = null!;
    private RenderContext renderContext = null!;

    protected override void Initialize(DependencyResolver dependencies, Intent intent)
    {
        logger = dependencies.Resolve<Logger>();
        contentManager = dependencies.Resolve<ContentManager>();
        renderContext = dependencies.Resolve<RenderContext>();

        if (intent != Intent.None)
        {
            dependencies.Resolve<UIUpdater>().Add(new IntentExecutor(intent, this));
        }
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
        Func<ServerNetworkInterface, Logger, ContentManager, RenderContext, ServerLobbyManager> lobbyManagerFactory
    )
    {
        var network = new ServerNetworkInterface();
        network.RegisterMessageHandler(new NetworkDebugMessageHandler(logger));
        var lobbyManager = lobbyManagerFactory(network, logger, contentManager, renderContext);
        Navigation!.Replace<Lobby, LobbyManager>(lobbyManager, this);
    }

    public void OnJoinGameButtonClicked() => Navigation!.Replace<LobbyList>(this);

    public void OnOptionsButtonClicked() => Navigation!.Replace<SettingsEditor>(this);

    public void OnQuitGameButtonClicked() => Navigation!.Exit();

    private sealed class IntentExecutor : UIUpdater.IUpdatable
    {
        private readonly Intent intent;
        private readonly MainMenu menu;

        public bool Deleted { get; private set; }

        public IntentExecutor(Intent intent, MainMenu menu)
        {
            this.intent = intent;
            this.menu = menu;
        }

        public void Update(UpdateEventArgs args)
        {
            switch (intent)
            {
                case Intent.QuickGame:
                    menu.OnQuickGameButtonClicked();
                    break;
                default:
                    menu.logger.Error?.Log($"Unsupported intent: {intent}");
                    break;
            }
            Deleted = true;
        }
    }
}
