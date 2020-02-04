using System;
using Bearded.TD.Utilities;
using Bearded.Utilities;
using Bearded.Utilities.SpaceTime;
using OpenTK;
using static Bearded.TD.Constants.Camera;

namespace Bearded.TD.Game.Camera
{
    sealed class GameCamera
    {
        private const float fovy = Mathf.PiOver2;
        private const float lowestZToRender = -10;
        private const float highestZToRender = 5;

        private ViewportSize viewportSize;

        private Position2 position;
        private float distance;

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

        private float nearPlaneDistance => Math.Max(Distance - highestZToRender, 0.1f);

        public float FarPlaneDistance => Distance - lowestZToRender;

        public Matrix4 ViewMatrix { get; private set; }

        public Matrix4 ProjectionMatrix { get; private set; }

        public GameCamera()
        {
            viewportSize = new ViewportSize(1280, 720);
            resetCameraPosition();
        }

        private void resetCameraPosition()
        {
            position = Position2.Zero;
            distance = ZDefault;
            recalculateMatrices();
        }

        private void recalculateMatrices()
        {
            ViewMatrix = calculateViewMatrix();
            ProjectionMatrix = calculateProjectionMatrix();
        }

        private Matrix4 calculateViewMatrix()
        {
            var eye = position.NumericValue.WithZ(distance);
            var target = position.NumericValue.WithZ();
            return Matrix4.LookAt(eye, target, Vector3.UnitY);
        }

        private Matrix4 calculateProjectionMatrix()
        {
            var zNear = nearPlaneDistance;
            var zFar = FarPlaneDistance;

            var yMax = zNear * Mathf.Tan(.5f * fovy);
            var yMin = -yMax;
            var xMax = yMax * viewportSize.AspectRatio;
            var xMin = yMin * viewportSize.AspectRatio;
            return Matrix4.CreatePerspectiveOffCenter(xMin, xMax, yMin, yMax, zNear, zFar);
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
