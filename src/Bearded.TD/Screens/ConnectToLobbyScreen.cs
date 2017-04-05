using amulware.Graphics;
using Bearded.TD.Networking;
using Bearded.TD.Rendering;
using Bearded.TD.UI;
using Bearded.TD.UI.Components;
using Bearded.TD.Utilities.Input;
using Bearded.Utilities;
using OpenTK;
using OpenTK.Input;

namespace Bearded.TD.Screens
{
    class ConnectToLobbyScreen : UIScreenLayer
    {
        private readonly Logger logger;
        private readonly InputManager inputManager;

        private readonly TextInput textInput;
        private ClientNetworkInterface networkInterface;

        public ConnectToLobbyScreen(ScreenLayerCollection parent, GeometryManager geometries, Logger logger, InputManager inputManager)
            : base(parent, geometries, .5f, .5f, true)
        {
            this.logger = logger;
            this.inputManager = inputManager;

            textInput = new TextInput(new Bounds(new FixedSizeDimension(Screen.X, 200, 0, .5f), new FixedSizeDimension(Screen.Y, 64, 0, .5f)));
        }

        public override void Update(UpdateEventArgs args)
        {
            base.Update(args);
            if (networkInterface == null)
                return;

            foreach (var msg in networkInterface.GetMessages())
                logger.Debug.Log(msg.MessageType);
        }

        public override bool HandleInput(UpdateEventArgs args, InputState inputState)
        {
            if (networkInterface != null)
                return true;

            if (inputState.InputManager.IsKeyHit(Key.Enter))
            {
                networkInterface = new ClientNetworkInterface(logger, textInput.Text);
                return true;
            }

            textInput.HandleInput(inputState);

            return true;
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
        }
    }
}