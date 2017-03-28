using System.Collections.Generic;
using amulware.Graphics;
using Bearded.TD.Rendering;
using Bearded.TD.UI;
using Bearded.TD.UI.Components;
using OpenTK;

namespace Bearded.TD.Screens
{
    abstract class UIScreenLayer : ScreenLayer
    {
        private readonly float originX;
        private readonly float originY;
        private readonly bool flipY;

        private Matrix4 viewMatrix;
        public override Matrix4 ViewMatrix => viewMatrix;

        protected GeometryManager Geometries { get; }
        protected Screen Screen { get; }

        private readonly List<UIComponent> components = new List<UIComponent>();

        protected UIScreenLayer(GeometryManager geometries) : this(geometries, .5f, 1, true)
        { }

        protected UIScreenLayer(GeometryManager geometries, float originX, float originY, bool flipY)
        {
            Geometries = geometries;
            Screen = Screen.GetCanvas();
            this.originX = originX;
            this.originY = originY;
            this.flipY = flipY;
        }

        public override void Update(UpdateEventArgs args)
        {
            components.ForEach(c => c.Update(args));
        }

        public override bool HandleInput(UpdateEventArgs args, InputState inputState)
        {
            components.ForEach(c => c.HandleInput(inputState));
            return true;
        }

        public override void Draw()
        {
            components.ForEach(c => c.Draw(Geometries));
        }

        protected void AddComponent(UIComponent component)
        {
            components.Add(component);
        }

        protected override void OnViewportSizeChanged()
        {
            var originCenter = new Vector3((.5f - originX) * ViewportSize.Width, (originY - .5f) * ViewportSize.Height, 0);
            viewMatrix = Matrix4.LookAt(
                new Vector3(0, 0, -.5f * ViewportSize.Height) + originCenter,
                Vector3.Zero + originCenter,
                Vector3.UnitY * (flipY ? -1 : 1));
            Screen.OnResize(ViewportSize);
        }
    }
}
