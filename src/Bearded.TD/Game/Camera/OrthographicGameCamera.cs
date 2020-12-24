using Bearded.Utilities;
using Bearded.Utilities.SpaceTime;
using OpenTK.Mathematics;

namespace Bearded.TD.Game.Camera
{
    sealed class OrthographicGameCamera : GameCamera
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

            return Matrix4.CreateOrthographicOffCenter(xMin, xMax, yMin, yMax, zNear, zFar);
        }

        public override Position2 TransformScreenToWorldPos(Vector2 screenPos)
        {
            // This is simple right now under the assumptions:
            // * The camera always looks straight down. That is, the camera eye and target both lie
            //   along the infinite extension of cameraPosition in the Z axis.
            // * The FoV is Pi/2
            return Position + Distance * GetNormalizedScreenPosition(screenPos);
        }
    }
}
