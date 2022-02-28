using Bearded.Utilities.SpaceTime;
using OpenTK.Mathematics;

namespace Bearded.TD.Game.Camera;

sealed class NewGameCameraController
{
    private readonly IGameCamera camera;

    public NewGameCameraController(IGameCamera camera)
    {
        this.camera = camera;
    }

    public ICameraDragOperation Grab(Vector2 mousePos)
    {
        var dragAnchor = camera.ScreenToWorldPosition(mousePos);
    }

    private sealed class DragOperation : ICameraDragOperation
    {
        private readonly NewGameCameraController controller;
        private readonly Position2 dragAnchor;

        public DragOperation(NewGameCameraController controller, Position2 dragAnchor)
        {
            this.controller = controller;
            this.dragAnchor = dragAnchor;
        }

        public void SetMousePosition(Vector2 mousePos)
        {
            var mousePosInWorld = controller.camera.ScreenToWorldPosition(mousePos);
            var error = dragAnchor - mousePosInWorld;

        }

        public void Release()
        {
            throw new System.NotImplementedException();
        }
    }
}

interface ICameraDragOperation
{
    void Release();
}
