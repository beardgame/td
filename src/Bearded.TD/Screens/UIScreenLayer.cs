using System.Collections.Generic;
using amulware.Graphics;
using Bearded.TD.Rendering;
using Bearded.TD.UI;
using Bearded.TD.UI.Components;
using Bearded.TD.Utilities.Input;
using OpenTK;

namespace Bearded.TD.Screens
{
    abstract class UIScreenLayer : ScreenLayer
    {
        private readonly bool flipY = true;
        
        private Matrix4 viewMatrix;
        public override Matrix4 ViewMatrix => viewMatrix;
        private Matrix4 viewProjectionInverse;

        protected GeometryManager Geometries { get; }
        protected Screen Screen { get; }

        private readonly List<UIComponent> components = new List<UIComponent>();

        protected UIScreenLayer(ScreenLayerCollection parent, GeometryManager geometries)
            : base(parent)
        {
            Geometries = geometries;
            Screen = Screen.GetCanvas();
        }

        public override void Update(UpdateEventArgs args)
        {
            components.ForEach(c => c.Update(args));
        }

        public sealed override void HandleInput(UpdateEventArgs args, InputState inputState)
        {
            var context = new InputContext(inputState, transformScreenToWorld);
            DoHandleInput(context);
            components.ForEach(c => c.HandleInput(context));
        }

        protected virtual bool DoHandleInput(InputContext input) => true;

        public override void Draw()
        {
            components.ForEach(c => c.Draw(Geometries));
        }

        protected void AddComponent(UIComponent component)
        {
            components.Add(component);
        }

        protected void RemoveComponent(UIComponent component)
        {
            components.Remove(component);
        }

        protected override void OnViewportSizeChanged()
        {
            var originCenter = new Vector3(
                .5f * ViewportSize.ScaledWidth,
                .5f * ViewportSize.ScaledHeight,
                0);
            var eye = new Vector3(0, 0, -.5f * ViewportSize.ScaledHeight) + originCenter;
            viewMatrix = Matrix4.LookAt(
                eye,
                originCenter,
                Vector3.UnitY * (flipY ? -1 : 1));
            viewProjectionInverse = Matrix4.Mult(ProjectionMatrix, ViewMatrix).Inverted();
            Screen.OnResize(ViewportSize);
        }

        private Vector2 transformScreenToWorld(Vector2 screenPos)
        {
            // Transform back to world position [-1, 1] x [-1, 1].
            var worldPos = unproject(screenPos);

            // Transform to world space of this screen.
            var transformedWorld = new Vector2(
                worldPos.X * .5f + .5f,
                worldPos.Y * .5f + .5f);
            if (flipY)
                transformedWorld.Y = 1 - transformedWorld.Y;
            transformedWorld.X *= ViewportSize.ScaledWidth;
            transformedWorld.Y *= ViewportSize.ScaledHeight;

            return new Vector2(transformedWorld.X, transformedWorld.Y);
        }

        private Vector4 unproject(Vector2 screenPos)
        {
            var vec = new Vector4(
                2f * screenPos.X / ViewportSize.Width - 1,
                -(2f * screenPos.Y / ViewportSize.Height - 1),
                0,
                1f);
            Vector4.Transform(vec, viewProjectionInverse);

            if (vec.W <= float.Epsilon && vec.W >= -float.Epsilon) return vec;

            vec.X /= vec.W;
            vec.Y /= vec.W;
            vec.Z /= vec.W;
            return vec;
        }
    }
}
