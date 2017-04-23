using amulware.Graphics;
using Bearded.TD.Networking;
using Bearded.TD.Networking.Lobby;
using Bearded.TD.Rendering;
using Bearded.TD.UI;
using Bearded.TD.UI.Components;
using Bearded.TD.Utilities.Input;
using Bearded.Utilities;
using Lidgren.Network;
using OpenTK;

namespace Bearded.TD.Screens
{
    class ConnectToLobbyScreen : UIScreenLayer
    {
        private readonly Logger logger;
        private readonly InputManager inputManager;

        private readonly TextInput textInput;
        private ClientNetworkInterface networkInterface;
        private string rejectionReason;

        public ConnectToLobbyScreen(ScreenLayerCollection parent, GeometryManager geometries, Logger logger, InputManager inputManager)
            : base(parent, geometries, .5f, .5f, true)
        {
            this.logger = logger;
            this.inputManager = inputManager;

            AddComponent(textInput = new TextInput(new Bounds(new FixedSizeDimension(Screen.X, 200, 0, .5f), new FixedSizeDimension(Screen.Y, 64, 0, .5f))));
            textInput.Submitted += tryConnect;
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
            }
        }

        private void handleStatusChange(NetIncomingMessage msg)
        {
            switch (msg.SenderConnection.Status)
            {
                case NetConnectionStatus.Connected:
                    goToLobby(msg);
                    break;
                case NetConnectionStatus.Disconnected:
                    msg.ReadByte(); // Read status byte.
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
            var clientInfo = new ClientInfo("a client");
            networkInterface = new ClientNetworkInterface(logger, textInput.Text, clientInfo);
        }

        private void goToLobby(NetIncomingMessage msg)
        {
            // We should be getting enough information from the lobby here to make our own lobby instance.
        }

        public override bool HandleInput(UpdateEventArgs args, InputState inputState)
        {
            return networkInterface != null || base.HandleInput(args, inputState);
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