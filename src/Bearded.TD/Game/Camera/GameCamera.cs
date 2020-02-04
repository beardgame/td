using System;
using Bearded.TD.Utilities;
using Bearded.Utilities;
using Bearded.Utilities.SpaceTime;
using OpenTK;

namespace Bearded.TD.Game.Camera
{
    abstract class GameCamera
    {
        private const float lowestZToRender = -10;
        private const float highestZToRender = 5;

        private Position2 position;
        private float distance;

        protected ViewportSize ViewportSize { get; private set; }

        public Position2 Position
        {
            get => position;
            set
            {
                position = value;
                recalculateMatrices();
            }
        }

        public float Distance
        {
            get => distance;
            set
            {
                distance = value;
                recalculateMatrices();
            }
        }

        protected float NearPlaneDistance => Math.Max(Distance - highestZToRender, 0.1f);
        public float FarPlaneDistance => Distance - lowestZToRender;
        public Matrix4 ViewMatrix { get; private set; }
        public Matrix4 ProjectionMatrix { get; private set; }

        protected GameCamera()
        {
            ViewportSize = new ViewportSize(1280, 720);
            resetCameraPosition();
        }

        private void resetCameraPosition()
        {
            position = Position2.Zero;
            distance = Constants.Camera.ZDefault;
            recalculateMatrices();
        }

        private void recalculateMatrices()
        {
            ViewMatrix = calculateViewMatrix();
            ProjectionMatrix = CalculateProjectionMatrix();
        }

        private Matrix4 calculateViewMatrix()
        {
            var eye = position.NumericValue.WithZ(distance);
            var target = position.NumericValue.WithZ();
            return Matrix4.LookAt(eye, target, Vector3.UnitY);
        }

        protected abstract Matrix4 CalculateProjectionMatrix();

        public void OnViewportSizeChanged(ViewportSize viewportSize)
        {
            ViewportSize = viewportSize;
        }

        public Position2 TransformScreenToWorldPos(Vector2 screenPos)
        {
            // This is simple right now under the assumptions:
            // * The camera always looks straight down. That is, the camera eye and target both lie
            //   along the infinite extension of cameraPosition in the Z axis.
            // * The FoV is Pi/2
            return position + distance * getNormalizedScreenPosition(screenPos);
        }

        private Difference2 getNormalizedScreenPosition(Vector2 screenPos)
        {
            var ret = new Vector2(
                2 * screenPos.X / ViewportSize.Width - 1,
                1 - 2 * screenPos.Y / ViewportSize.Height
            );
            ret.X *= ViewportSize.AspectRatio;
            return new Difference2(ret);
        }
    }
}
