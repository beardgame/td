using Bearded.TD.Rendering;
using OpenTK;

namespace Bearded.TD.Screens
{
    abstract class UIScreenLayer : ScreenLayer
    {
        private const float baseWidth = 1280;
        private const float baseHeight = 720;

        private readonly float originX;
        private readonly float originY;
        private readonly bool flipY;

        private Matrix4 viewMatrix;
        public override Matrix4 ViewMatrix => viewMatrix;

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

        protected override void OnViewportSizeChanged()
        {
            var originCenter = new Vector3((originX + .5f) * ViewportSize.Width, (originY - .5f) * ViewportSize.Height, 0);
            viewMatrix = Matrix4.LookAt(
                new Vector3(0, 0, -.5f * ViewportSize.Height) + originCenter,
                Vector3.Zero + originCenter,
                Vector3.UnitY * (flipY ? -1 : 1));
        }
    }
}
