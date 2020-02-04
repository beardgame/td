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
            // screenPos = new Vector2(ViewportSize.Width, ViewportSize.Height) / 2;

            var projectionView = ProjectionMatrix * ViewMatrix;
            var projectionViewInverse = Matrix4.Invert(projectionView);

            var normalizedScreenPos = new Vector4(
                2 * screenPos.X / ViewportSize.Width - 1, // window x scaled to [-1, 1]
                1 - 2 * screenPos.Y / ViewportSize.Height, // window y scaled to [-1, 1]
                0, // target z scaled to [-1, 1] where -1 = zNear and 1 = zFar
                1);

            var rawWorldPos = projectionViewInverse * normalizedScreenPos;
            var worldPos = rawWorldPos.Xyz / rawWorldPos.W;

            // Ray-tracing with the z=0 plane means we can take a shortcut.
            var eye = position.WithZ(Distance).NumericValue;
            var diff = worldPos - eye;
            var diffNormalized = diff / diff.Length;
            var t = Distance / diffNormalized.Z;
            var result = eye - t * diffNormalized;
            Console.WriteLine(result);
            return new Position2(result.Xy);
        }
    }
}
