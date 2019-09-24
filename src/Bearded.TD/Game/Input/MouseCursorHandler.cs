﻿using Bearded.TD.Game.World;
using Bearded.TD.Tiles;
using Bearded.TD.Utilities.Input;
using Bearded.Utilities.SpaceTime;

namespace Bearded.TD.Game.Input
{
    sealed class MouseCursorHandler : ICursorHandler
    {
        private readonly GameCamera camera;
        private readonly MouseCameraController cameraController;
        private readonly Level level;
        private TileSelection tileSelection;

        public ActionState Click { get; private set; }
        public ActionState Cancel { get; private set; }
        public Position2 CursorPosition { get; private set; }

        public PositionedFootprint CurrentFootprint { get; private set; }

        public MouseCursorHandler(GameCamera camera, GameCameraController cameraController, Level level)
        {
            this.camera = camera;
            this.cameraController = new MouseCameraController(cameraController);
            this.level = level;
            tileSelection = TileSelection.FromFootprints(FootprintGroup.Single);
        }

        public void HandleInput(InputState input)
        {
            cameraController.HandleInput(input);
            CursorPosition = camera.TransformScreenToWorldPos(input.Mouse.Position);
            Click = input.Mouse.Click;
            Cancel = input.Mouse.Cancel;
            updateFootprint();
        }

        public void SetTileSelection(TileSelection tileSelection)
        {
            this.tileSelection = tileSelection;
            updateFootprint();
        }

        private void updateFootprint()
        {
            CurrentFootprint = tileSelection.GetPositionedFootprint(level, CursorPosition);
        }
    }
}
