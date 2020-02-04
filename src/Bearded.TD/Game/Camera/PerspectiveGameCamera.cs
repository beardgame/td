using Bearded.Utilities;
using OpenTK;

namespace Bearded.TD.Game.Camera
{
    sealed class PerspectiveGameCamera : GameCamera
    {
        private const float fovy = Mathf.PiOver2;

        protected override Matrix4 CalculateProjectionMatrix()
        {
            var zNear = NearPlaneDistance;
            var zFar = FarPlaneDistance;

            var yMax = zNear * Mathf.Tan(.5f * fovy);
            var yMin = -yMax;
            var xMax = yMax * ViewportSize.AspectRatio;
            var xMin = yMin * ViewportSize.AspectRatio;
            return Matrix4.CreatePerspectiveOffCenter(xMin, xMax, yMin, yMax, zNear, zFar);
        }
    }
}
