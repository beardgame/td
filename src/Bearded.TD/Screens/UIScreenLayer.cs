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

        private Vector3 eye;
        private Matrix4 viewMatrix;
        public override Matrix4 ViewMatrix => viewMatrix;
        private Matrix4 viewProjectionInverse;

        protected GeometryManager Geometries { get; }
        protected Screen Screen { get; }

        private readonly List<UIComponent> components = new List<UIComponent>();

        protected UIScreenLayer(ScreenLayerCollection parent, GeometryManager geometries) : this(parent, geometries, .5f, 1, true)
        { }

        protected UIScreenLayer(ScreenLayerCollection parent, GeometryManager geometries, float originX, float originY, bool flipY) : base(parent)
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
            if (inputState.InputManager.RightMouseHit)
                System.Console.WriteLine($"mouse screen pos for {GetType()}: {TransformScreenToWorld(inputState.InputManager.MousePosition)}");
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
            var originCenter = new Vector3(
                (.5f - originX) * ViewportSize.ScaledWidth,
                (originY - .5f) * ViewportSize.ScaledHeight,
                0);
            eye = new Vector3(0, 0, -.5f * ViewportSize.ScaledHeight) + originCenter;
            viewMatrix = Matrix4.LookAt(
                eye,
                originCenter,
                Vector3.UnitY * (flipY ? -1 : 1));
            viewProjectionInverse = Matrix4.Mult(ProjectionMatrix, ViewMatrix).Inverted();
            Screen.OnResize(ViewportSize);
        }

        public Vector2 TransformScreenToWorld(Vector2 screenPos)
        {
            screenPos *= (float)ViewportSize.ScaledWidth / ViewportSize.Width;

            // Transform back to world position.
            var worldPos3D = Vector3.TransformPosition(new Vector3(screenPos.X, screenPos.Y, 0), viewProjectionInverse);

            // Correct x/y
            worldPos3D.X /= ViewportSize.AspectRatio;
            if (flipY)
                worldPos3D.Y *= -1;
            worldPos3D.X -= originX * ViewportSize.ScaledWidth;
            worldPos3D.Y -= originY * ViewportSize.ScaledHeight;

            // Project on z=0 plane.
            var t = -eye.Z / (worldPos3D.Z - eye.Z);
            var projected = eye + (worldPos3D - eye) * t;
            return projected.Xy;
        }
    }
}
