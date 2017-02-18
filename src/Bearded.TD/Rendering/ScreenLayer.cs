using Bearded.Utilities.Math;
using OpenTK;

namespace Bearded.TD.Rendering
{
    abstract class ScreenLayer
    {
        private const float fovy = Mathf.PiOver2;
        private const float zNear = .1f;
        private const float zFar = 1024f;
        private const float aspectRatio = 16f / 9f;

        public abstract void Draw();

        public abstract Matrix4 GetViewMatrix();

        public virtual Matrix4 GetProjectionMatrix()
        {
            var yMax = zNear * Mathf.Tan(.5f * fovy);
            var yMin = -yMax;
            var xMax = yMax * aspectRatio;
            var xMin = yMin * aspectRatio;
            return Matrix4.CreatePerspectiveOffCenter(xMin, xMax, yMin, yMax, zNear, zFar);
        }
    }
}
