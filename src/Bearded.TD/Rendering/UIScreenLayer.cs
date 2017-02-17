using OpenTK;

namespace Bearded.TD.Rendering
{
    abstract class UIScreenLayer : ScreenLayer
    {
        private const float baseWidth = 1280;
        private const float baseHeight = 720;

        private readonly float originX;
        private readonly float originY;
        private readonly bool flipY;

        protected GeometryManager Geometries { get; }

        protected UIScreenLayer(GeometryManager geometries) : this(geometries, .5f, 1, true)
        { }

        protected UIScreenLayer(GeometryManager geometries, float originX, float originY, bool flipY)
        {
            Geometries = geometries;
            this.originX = originX;
            this.originY = originY;
            this.flipY = flipY;
        }

        public override Matrix4 GetViewMatrix()
        {
            var originCenter = new Vector3((originX - .5f) * baseWidth, (originY - .5f) * baseHeight, 0);
            return Matrix4.LookAt(
                new Vector3(0, 0, -.5f * baseHeight) + originCenter,
                Vector3.Zero + originCenter,
                Vector3.UnitY * (flipY ? -1 : 1));
        }
    }
}
