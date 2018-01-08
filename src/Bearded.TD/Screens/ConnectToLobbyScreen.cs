using System.Collections.Generic;
using amulware.Graphics;
using Bearded.TD.Game.Players;
using Bearded.TD.Meta;
using Bearded.TD.Mods;
using Bearded.TD.Networking;
using Bearded.TD.Rendering;
using Bearded.TD.UI;
using Bearded.TD.UI.Components;
using Bearded.TD.UI.Model.Lobby;
using Bearded.TD.Utilities.Collections;
using Bearded.TD.Utilities.Input;
using Bearded.Utilities.IO;
using Lidgren.Network;
using OpenTK;

namespace Bearded.TD.Screens
{
    class ConnectToLobbyScreen : UIScreenLayer
    {
        private const string defaultPlayerName = "a client";

        private readonly Logger logger;
        private readonly InputManager inputManager;
        private readonly ContentManager contentManager;
        private readonly ClientNetworkInterface networkInterface;

		private readonly List<Proto.Lobby> availableLobbies = new List<Proto.Lobby>();
        private readonly HashSet<long> availableLobbySet = new HashSet<long>();
        private readonly LinkedList<Button> lobbyButtons = new LinkedList<Button>();

        private readonly TextInput textInput;
        private readonly string playerName;
        private string rejectionReason;

        public ConnectToLobbyScreen(ScreenLayerCollection parent, GeometryManager geometries, Logger logger, InputManager inputManager,
            ContentManager contentManager)
            : base(parent, geometries)
        {
            this.logger = logger;
            this.inputManager = inputManager;
            networkInterface = new ClientNetworkInterface(logger);

            AddComponent(new Button(
                Bounds.AnchoredBox(Screen, 0, 1, new Vector2(100, 24), new Vector2(-12, -12)),
                goToMenu,
                "back to menu"));
            AddComponent(new Button(
                Bounds.AnchoredBox(Screen, 0, 1, new Vector2(100, 24), new Vector2(-118, -12)),
                refreshLobbies,
                "refresh lobby list"));

            textInput = new TextInput(
                Bounds.AnchoredBox(Screen, 0, 0, new Vector2(200, 64), new Vector2(12, 12)));
            this.contentManager = contentManager;

            textInput = new TextInput(new Bounds(
                new FixedSizeDimension(Screen.X, 200, 0, .5f), new FixedSizeDimension(Screen.Y, 64, 0, .5f)));
            textInput.Focus();
            AddComponent(textInput);
            textInput.Submitted += tryManualConnect;

            playerName = UserSettings.Instance.Misc.Username?.Length > 0
                ? UserSettings.Instance.Misc.Username
                : defaultPlayerName;
            if (UserSettings.Instance.Misc.SavedNetworkAddress?.Length > 0)
                textInput.Text = UserSettings.Instance.Misc.SavedNetworkAddress;

            // Kick off retrieval of lobbies.
            refreshLobbies();
        }

        public override void Update(UpdateEventArgs args)
        {
            base.Update(args);

            foreach (var msg in networkInterface.GetMessages())
            {
                switch (msg.MessageType)
                {
                    case NetIncomingMessageType.StatusChanged:
                        handleStatusChange(msg);
                        break;
                    case NetIncomingMessageType.UnconnectedData:
                        // Data coming from an unconnected source. Must be master server.
                        handleIncomingLobby(Proto.Lobby.Parser.ParseFrom(msg.ReadBytes(msg.LengthBytes)));
                        break;
                }
                if (Destroyed) return;
            }
        }

        private void handleStatusChange(NetIncomingMessage msg)
        {
            msg.ReadByte(); // Read status byte.
            switch (msg.SenderConnection.Status)
            {
                case NetConnectionStatus.Connected:
                    goToLobby(msg);
                    return;
                case NetConnectionStatus.Disconnected:
                    rejectionReason = msg.ReadString();
                    logger.Info.Log(string.IsNullOrEmpty(rejectionReason)
                        ? "Disconnected"
                        : $"Disconnected with reason: {rejectionReason}");
                    networkInterface.Shutdown();
                    break;
            }
        }

        private void handleIncomingLobby(Proto.Lobby lobby)
        {
            if (availableLobbySet.Contains(lobby.Id))
            {
                availableLobbies.RemoveAll(l => l.Id == lobby.Id);
            }
            availableLobbies.Add(lobby);
            availableLobbySet.Add(lobby.Id);
            updateLobbyButtons();
        }

        private void updateLobbyButtons()
        {
            const float buttonHeight = 24;
            const float buttonPadding = 6;

            foreach (var button in lobbyButtons)
                RemoveComponent(button);
            lobbyButtons.Clear();

            foreach (var (lobby, i) in availableLobbies.Indexed())
            {
                var bounds = new Bounds(
                    new ScalingDimension(Screen.X, .5f, .5f),
                    new AnchoredFixedSizeDimension(Screen.Y, 0, buttonHeight, buttonPadding + i * (buttonHeight + buttonPadding))
                );
                var button = new Button(
                    bounds,
                    () => tryLobbyConnect(lobby),
                    lobby.Name
                );
                AddComponent(button);
                lobbyButtons.AddLast(button);
            }
        }

        private void tryManualConnect(string host)
        {
            networkInterface.Connect(host, new ClientInfo(playerName));
        }

        private void tryLobbyConnect(Proto.Lobby lobby)
        {
            networkInterface.Master.ConnectToLobby(lobby.Id);
        }

        private void goToMenu()
        {
            Parent.AddScreenLayerOnTopOf(this, new StartScreen(Parent, Geometries, logger, inputManager, contentManager));
            Destroy();
        }

        private void refreshLobbies()
        {
            networkInterface.Master.ListLobbies();
        }

        private void goToLobby(NetIncomingMessage msg)
        {
            UserSettings.Instance.Misc.SavedNetworkAddress = textInput.Text;
            UserSettings.Save(logger);
            var info = LobbyPlayerInfo.FromBuffer(msg.SenderConnection.RemoteHailMessage);
            var lobbyManager =
                new ClientLobbyManager(networkInterface, new Player(info.Id, playerName), logger, contentManager);
            Parent.AddScreenLayerOnTopOf(this, new LobbyScreen(Parent, Geometries, lobbyManager, inputManager));
            Destroy();
        }

        public override void Draw()
        {
            base.Draw();

            if (rejectionReason != null)
			{
				var txtGeo = Geometries.ConsoleFont;
                txtGeo.Color = Color.Red;
                txtGeo.DrawString(64 * Vector2.UnitY, rejectionReason, .5f, .5f);
            }
        }
    }
}
