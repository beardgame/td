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
            var pixelStep = 1f / Constants.Rendering.PixelsPerTileLevelResolution;

            var p = position.NumericValue;

            p = new Vector2(
                Mathf.FloorToInt(p.X / pixelStep) * pixelStep,
                Mathf.FloorToInt(p.Y / pixelStep) * pixelStep
                );

            var eye = p.WithZ(distance);
            var target = p.WithZ();
            return Matrix4.LookAt(eye, target, Vector3.UnitY);
        }

        protected abstract Matrix4 CalculateProjectionMatrix();

        public void OnViewportSizeChanged(ViewportSize viewportSize)
        {
            ViewportSize = viewportSize;
        }

        public abstract Position2 TransformScreenToWorldPos(Vector2 screenPos);

        protected Difference2 GetNormalizedScreenPosition(Vector2 screenPos)
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
