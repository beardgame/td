using System;
using Bearded.TD.Mods;
using Bearded.TD.Networking;
using Bearded.TD.Rendering;
using Bearded.TD.UI;
using Bearded.TD.UI.Components;
using Bearded.TD.UI.Model.Lobby;
using Bearded.TD.Utilities.Input;
using Bearded.Utilities.IO;
using OpenTK;

namespace Bearded.TD.Screens
{
    class StartScreen : UIScreenLayer
    {
        private readonly Logger logger;
        private readonly InputManager inputManager;
        private readonly ContentManager contentManager;

        public StartScreen(ScreenLayerCollection parent, GeometryManager geometries, Logger logger, InputManager inputManager, ContentManager contentManager)
            : base(parent, geometries)
        {
            this.logger = logger;
            this.inputManager = inputManager;
            this.contentManager = contentManager;

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
                new ServerLobbyManager(new ServerNetworkInterface(logger), logger, contentManager),
                inputManager));
            Destroy();
        }

        private void startConnect()
        {
            Parent.AddScreenLayerOnTopOf(this, new ConnectToLobbyScreen(Parent, Geometries, logger, inputManager, contentManager));
            Destroy();
        }
    }
}