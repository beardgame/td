using Bearded.TD.Utilities;
using Bearded.Utilities;
using Bearded.Utilities.SpaceTime;
using OpenTK;
using static Bearded.TD.Constants.Camera;

namespace Bearded.TD.Game
{
    class GameCamera
    {
        private ViewportSize viewportSize;

        private Position2 position;
        private float distance;

        public Position2 Position
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
            position = Position2.Zero;
            distance = ZDefault;
            recalculateViewMatrix();
        }

        private void recalculateViewMatrix()
        {
            var eye = position.NumericValue.WithZ(distance);
            var target = position.NumericValue.WithZ();
            ViewMatrix = Matrix4.LookAt(eye, target, Vector3.UnitY);
        }

        public void OnViewportSizeChanged(ViewportSize viewportSize)
        {
            this.viewportSize = viewportSize;
        }

        public Position2 TransformScreenToWorldPos(Vector2 screenPos)
        {
            // This is simple right now under the assumptions:
            // * The camera always looks straight down. That is, the camera eye and target both lie
            //   along the infinite extension of cameraPosition in the Z axis.
            // * The FoV is Pi/2
            return position + distance * getNormalisedScreenPosition(screenPos);
        }

        private Difference2 getNormalisedScreenPosition(Vector2 screenPos)
        {
            var ret = new Vector2(
                2 * screenPos.X / viewportSize.Width - 1,
                1 - 2 * screenPos.Y / viewportSize.Height
            );
            ret.X *= viewportSize.AspectRatio;
            return new Difference2(ret);
        }
    }
}
