using System;
using Bearded.TD.Networking;
using Bearded.TD.Networking.Lobby;
using Bearded.TD.Rendering;
using Bearded.TD.UI;
using Bearded.TD.UI.Components;
using Bearded.TD.Utilities.Input;
using Bearded.Utilities;
using OpenTK;

namespace Bearded.TD.Screens
{
    class StartScreen : UIScreenLayer
    {
        private readonly Logger logger;
        private readonly InputManager inputManager;

        public StartScreen(ScreenLayerCollection parent, GeometryManager geometries, Logger logger, InputManager inputManager)
            : base(parent, geometries)
        {
            this.logger = logger;
            this.inputManager = inputManager;

            AddComponent(new Menu(
                Bounds.AnchoredBox(Screen, BoundsAnchor.End, BoundsAnchor.End, new Vector2(220, 200), -25 * Vector2.One),
                new Func<Bounds, FocusableUIComponent> []
                {
                    bounds => new Button(bounds, startServerLobby, "Start lobby", 48, .5f),
                    bounds => new Button(bounds, startConnect, "Join lobby", 48, .5f)
                }));
        }

        protected override bool DoHandleInput(InputContext input)
        {
            base.DoHandleInput(input);
            return false;
        }

        private void startServerLobby()
        {
            Parent.AddScreenLayerOnTopOf(this, new LobbyScreen(
                Parent,
                Geometries,
                new ServerLobbyManager(new ServerNetworkInterface(logger), logger),
                inputManager));
            Destroy();
        }

        private void startConnect()
        {
            Parent.AddScreenLayerOnTopOf(this, new ConnectToLobbyScreen(Parent, Geometries, logger, inputManager));
            Destroy();
        }
    }
}