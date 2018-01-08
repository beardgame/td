﻿using System.Collections.Generic;
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

        private Color statusColor;
        private string statusText;

        public ConnectToLobbyScreen(
            ScreenLayerCollection parent, GeometryManager geometries, Logger logger, InputManager inputManager,
            ContentManager contentManager)
            : base(parent, geometries)
        {
            this.logger = logger;
            this.inputManager = inputManager;
            this.contentManager = contentManager;
            networkInterface = new ClientNetworkInterface(logger);

            AddComponent(new Button(
                Bounds.AnchoredBox(Screen, 0, 1, new Vector2(220, 50), new Vector2(10, -10)),
                goToMenu,
                "back to menu",
                32));
            AddComponent(new Button(
                Bounds.AnchoredBox(Screen, 0, 1, new Vector2(220, 50), new Vector2(240, -10)),
                refreshLobbies,
                "refresh lobby list",
                32));
            textInput = new TextInput(
                Bounds.AnchoredBox(Screen, 0, 1, new Vector2(220, 32), new Vector2(10, -90)));
            AddComponent(new Button(
                Bounds.AnchoredBox(Screen, 0, 1, new Vector2(220, 32), new Vector2(240, -90)),
                () => tryManualConnect(textInput.Text),
                "join manually",
                24));

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
                    var rejectionReason = msg.ReadString();
                    logger.Info.Log(string.IsNullOrEmpty(rejectionReason)
                        ? "Disconnected"
                        : $"Disconnected with reason: {rejectionReason}");
                    setStatusError(rejectionReason);
                    networkInterface.Shutdown();
                    break;
            }
        }

        private void handleIncomingLobby(Proto.Lobby lobby)
        {
            clearStatus();
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
            setStatusInfo($"Attempting to connect to {host}...");
            networkInterface.Connect(host, new ClientInfo(playerName));
        }

        private void tryLobbyConnect(Proto.Lobby lobby)
        {
            setStatusInfo($"Attempting to connect to lobby {lobby.Name}...");
            networkInterface.Master.ConnectToLobby(lobby.Id);
        }

        private void goToMenu()
        {
            Parent.AddScreenLayerOnTopOf(this, new StartScreen(Parent, Geometries, logger, inputManager, contentManager));
            Destroy();
        }

        private void refreshLobbies()
        {
            setStatusInfo("Refreshing lobbies...");
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

        private void setStatusInfo(string text)
        {
            statusText = text;
            statusColor = Color.LightBlue;
        }

        private void setStatusError(string text)
        {
            statusText = text;
            statusColor = Color.Red;
        }

        private void clearStatus()
        {
            statusText = null;
        }

        public override void Draw()
        {
            base.Draw();

            if (statusText != null)
			{
				var txtGeo = Geometries.ConsoleFont;
                txtGeo.Color = statusColor;
                txtGeo.DrawString(new Vector2(Screen.XStart, Screen.YStart) + 10 * Vector2.One, statusText);
            }
        }
    }
}
