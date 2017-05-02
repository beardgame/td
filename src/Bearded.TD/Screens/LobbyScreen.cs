﻿using System;
using amulware.Graphics;
using Bearded.TD.Game;
using Bearded.TD.Game.UI;
using Bearded.TD.Networking;
using Bearded.TD.Networking.Lobby;
using Bearded.TD.Rendering;
using Bearded.TD.UI;
using Bearded.TD.UI.Components;
using Bearded.TD.Utilities.Input;
using OpenTK;
using OpenTK.Input;

namespace Bearded.TD.Screens
{
    class LobbyScreen : UIScreenLayer
    {
        private readonly LobbyManager lobbyManager;
        private readonly InputManager inputManager;
        
        public LobbyScreen(
                ScreenLayerCollection parent, GeometryManager geometries,
                LobbyManager lobbyManager, InputManager inputManager)
            : base(parent, geometries, 0, 1, true)
        {
            this.lobbyManager = lobbyManager;
            this.inputManager = inputManager;

            lobbyManager.Game.GameStatusChanged += onGameStatusChanged;

            AddComponent(new ChatComponent(new Bounds(
                new ScalingDimension(Screen.X, .3f, .7f), new ScalingDimension(Screen.Y)), lobbyManager.Game));
        }

        public override bool HandleInput(UpdateEventArgs args, InputState inputState)
        {
            if (inputState.InputManager.IsKeyPressed(Key.ShiftLeft) && inputState.InputManager.IsKeyHit(Key.Enter))
                lobbyManager.ToggleReadyState();
            else
                return base.HandleInput(args, inputState);

            return false;
        }

        public override void Update(UpdateEventArgs args)
        {
            base.Update(args);
            lobbyManager.Update(args);
        }

        public override void Draw()
        {
            base.Draw();

            var txtGeo = Geometries.ConsoleFont;

            txtGeo.Color = Color.White;
            txtGeo.SizeCoefficient = Vector2.One;
            txtGeo.Height = 48;
            txtGeo.DrawString(new Vector2(16, 16), "Press [shift+enter] to start");
            txtGeo.DrawString(new Vector2(16, 80), $"Player count: {lobbyManager.Game.Players.Count}");
            for (var i = 0; i < lobbyManager.Game.Players.Count; i++)
            {
                var p = lobbyManager.Game.Players[i];
                txtGeo.Color = p.ConnectionState == PlayerConnectionState.Ready ? Color.Lime : Color.Gold;
                txtGeo.DrawString(new Vector2(16, 144 + i * 64), p.Name);
            }
        }

        private void onGameStatusChanged(GameStatus gameStatus)
        {
            if (gameStatus != GameStatus.Loading)
                throw new Exception("Unexpected game status change.");
            beginLoading();
        }

        private void beginLoading()
        {
            Parent.AddScreenLayerOnTopOf(this, new LoadingScreen(Parent, Geometries, lobbyManager.GetLoadingManager(), inputManager));
            lobbyManager.Game.GameStatusChanged -= onGameStatusChanged;
            Destroy();
        }
    }
}