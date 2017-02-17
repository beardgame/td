using Bearded.Utilities.Math;
using OpenTK;

namespace Bearded.TD.Rendering
{
    class ScreenLayer
    {
        private const float fovy = Mathf.PiOver4;
        private const float zNear = .1f;
        private const float zFar = 256f;
        private const float aspectRatio = 16f / 9f;

        private const float baseWidth = 1280;
        private const float baseHeight = 720;

        private readonly float originX;
        private readonly float originY;
        private readonly bool flipY;

        public ScreenLayer() : this(.5f, 1, true)
        { }

        // Origin = bottom left.
        // Default y direction = up.
        // Flipping y is done after moving origin.
        public ScreenLayer(float originX, float originY, bool flipY)
        {
            this.originX = originX;
            this.originY = originY;
            this.flipY = flipY;
        }

        public Matrix4 GetViewMatrix()
        {
            var originCenter = new Vector3((originX - .5f) * baseWidth, (originY - .5f) * baseHeight, 0);
            return Matrix4.LookAt(
                new Vector3(0, 0, -.5f * baseHeight) + originCenter,
                Vector3.Zero + originCenter,
                Vector3.UnitY * (flipY ? -1 : 1));
        }

        public Matrix4 GetProjectionMatrix()
        {
            var yMax = zNear * Mathf.Tan(.5f * fovy);
            var yMin = -yMax;
            var xMax = yMax * aspectRatio;
            var xMin = yMin * aspectRatio;
            return Matrix4.CreatePerspectiveOffCenter(xMin, xMax, yMin, yMax, zNear, zFar);
        }
    }
}
