using amulware.Graphics;
using Bearded.TD.Game.Players;
using Bearded.TD.Meta;
using Bearded.TD.Networking;
using Bearded.TD.Rendering;
using Bearded.TD.UI;
using Bearded.TD.UI.Components;
using Bearded.TD.UI.Model.Lobby;
using Bearded.TD.Utilities.Input;
using Bearded.Utilities;
using Lidgren.Network;
using OpenTK;

namespace Bearded.TD.Screens
{
    class ConnectToLobbyScreen : UIScreenLayer
    {
        private const string defaultPlayerName = "a client";

        private readonly Logger logger;
        private readonly InputManager inputManager;

        private readonly TextInput textInput;
        private ClientNetworkInterface networkInterface;
        private readonly string playerName;
        private string rejectionReason;

        public ConnectToLobbyScreen(ScreenLayerCollection parent, GeometryManager geometries, Logger logger, InputManager inputManager)
            : base(parent, geometries, .5f, .5f, true)
        {
            this.logger = logger;
            this.inputManager = inputManager;

            textInput = new TextInput(new Bounds(
                new FixedSizeDimension(Screen.X, 200, 0, .5f), new FixedSizeDimension(Screen.Y, 64, 0, .5f)));
            textInput.Focus();
            AddComponent(textInput);
            textInput.Submitted += tryConnect;

            playerName = UserSettings.Instance.Misc.Username?.Length > 0
                ? UserSettings.Instance.Misc.Username
                : defaultPlayerName;
            if (UserSettings.Instance.Misc.SavedNetworkAddress?.Length > 0)
                textInput.Text = UserSettings.Instance.Misc.SavedNetworkAddress;
        }

        public override void Update(UpdateEventArgs args)
        {
            base.Update(args);
            if (networkInterface == null)
                return;

            foreach (var msg in networkInterface.GetMessages())
            {
                switch (msg.MessageType)
                {
                    case NetIncomingMessageType.StatusChanged:
                        handleStatusChange(msg);
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
                    networkInterface = null;
                    break;
            }
        }

        private void tryConnect(string host)
        {
            var clientInfo = new ClientInfo(playerName);
            networkInterface = new ClientNetworkInterface(logger, textInput.Text, clientInfo);
        }

        private void goToLobby(NetIncomingMessage msg)
        {
            UserSettings.Instance.Misc.SavedNetworkAddress = textInput.Text;
            UserSettings.Save(logger);
            var info = LobbyPlayerInfo.FromBuffer(msg.SenderConnection.RemoteHailMessage);
            var lobbyManager =
                new ClientLobbyManager(networkInterface, new Player(info.Id, playerName), logger);
            Parent.AddScreenLayerOnTopOf(this, new LobbyScreen(Parent, Geometries, lobbyManager, inputManager));
            Destroy();
        }

        protected override bool DoHandleInput(InputContext input)
        {
            return networkInterface != null || base.DoHandleInput(input);
        }

        public override void Draw()
        {
            var txtGeo = Geometries.ConsoleFont;

            txtGeo.Color = Color.White;
            txtGeo.SizeCoefficient = Vector2.One;
            txtGeo.Height = 48;
            txtGeo.DrawString(-64 * Vector2.UnitY, "Type host IP and press [enter]", .5f, .5f);

            textInput.Draw(Geometries);

            if (networkInterface != null)
                txtGeo.DrawString(64 * Vector2.UnitY, "Trying to connect...", .5f, .5f);
            else if (rejectionReason != null)
            {
                txtGeo.Color = Color.Red;
                txtGeo.DrawString(64 * Vector2.UnitY, rejectionReason, .5f, .5f);
            }
        }
    }
}
