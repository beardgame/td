using amulware.Graphics;
using Bearded.TD.Game.World;
using Bearded.TD.Tiles;
using Bearded.TD.Utilities.Input;
using Bearded.Utilities.SpaceTime;

namespace Bearded.TD.Game.Input
{
    sealed class MouseCursorHandler : ICursorHandler
    {
        private readonly MouseCameraController cameraController;
        private readonly GameCamera camera;
        private readonly Level level;
        private TileSelection tileSelection;

        public ActionState Click { get; private set; }
        public ActionState Cancel { get; private set; }
        public Position2 CursorPosition { get; private set; }

        public PositionedFootprint CurrentFootprint { get; private set; }

        public MouseCursorHandler(GameCamera camera, Level level)
        {
            cameraController = new MouseCameraController(camera, level.Radius);
            this.camera = camera;
            this.level = level;
            tileSelection = TileSelection.FromFootprints(FootprintGroup.Single);
        }

        public void Update(UpdateEventArgs args, InputState input)
        {
            cameraController.HandleInput(args, input);
            CursorPosition = new Position2(camera.TransformScreenToWorldPos(input.Mouse.Position));
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
