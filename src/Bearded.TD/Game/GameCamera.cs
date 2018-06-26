using Bearded.TD.Utilities;
using Bearded.Utilities;
using OpenTK;
using static Bearded.TD.Constants.Camera;

namespace Bearded.TD.Game
{
    class GameCamera
    {
        private ViewportSize viewportSize;

        private Vector2 position;
        private float distance;

        public Vector2 Position
        {
            get => position;
            set
            {
                position = value;
                recalculateViewMatrix();
            }
        }

        public float Distance
        {
            get => distance;
            set
            {
                distance = value;
                recalculateViewMatrix();
            }
        }
        
        public Matrix4 ViewMatrix { get; private set; }

        public GameCamera()
        {
            viewportSize = new ViewportSize(1280, 720);
            resetCameraPosition();
        }

        private void resetCameraPosition()
        {
            position = Vector2.Zero;
            distance = ZDefault;
            recalculateViewMatrix();
        }

        private void recalculateViewMatrix()
        {
            var eye = position.WithZ(distance);
            var target = position.WithZ();
            ViewMatrix = Matrix4.LookAt(eye, target, Vector3.UnitY);
        }

        public void OnViewportSizeChanged(ViewportSize viewportSize)
        {
            this.viewportSize = viewportSize;
        }

        public Vector2 TransformScreenToWorldPos(Vector2 screenPos)
        {
            // This is simple right now under the assumptions:
            // * The camera always looks straight down. That is, the camera eye and target both lie
            //   along the infinite extension of cameraPosition in the Z axis.
            // * The FoV is Pi/2
            return position + distance * getNormalisedScreenPosition(screenPos);
        }

        private Vector2 getNormalisedScreenPosition(Vector2 screenPos)
        {
            var ret = new Vector2(
                2 * screenPos.X / viewportSize.Width - 1,
                1 - 2 * screenPos.Y / viewportSize.Height
            );
            ret.X *= viewportSize.AspectRatio;
            return ret;
        }
    }
}
