using System;
using amulware.Graphics;
using Bearded.TD.Rendering;
using Bearded.TD.Screens;
using OpenTK;

namespace Bearded.TD.UI.Components
{
    class Button : UIComponent
    {
        private const float padding = 8f;

        private readonly UIScreenLayer screen;
        private readonly Action action;
        private readonly string text;
        private readonly float fontSize;
        private readonly float textAlign;
        private bool hovered;

        public Button(UIScreenLayer screen, Bounds bounds, Action action, string text, float fontSize, float textAlign = 0f) : base(bounds)
        {
            this.screen = screen;
            this.action = action;
            this.text = text;
            this.fontSize = fontSize;
            this.textAlign = textAlign;
        }

        public override void HandleInput(InputState inputState)
        {
            base.HandleInput(inputState);
            hovered = Bounds.Contains(screen.TransformScreenToWorld(inputState.InputManager.MousePosition));
            if (inputState.InputManager.LeftMouseHit && hovered)
                action();
        }

        public override void Draw(GeometryManager geometries)
        {
            var primitivesGeo = geometries.Primitives;

            primitivesGeo.Color = Color.HotPink * (hovered ? .2f : .05f);
            primitivesGeo.DrawRectangle(Bounds.XStart, Bounds.YStart, Bounds.Width, Bounds.Height);


            var fontGeo = geometries.UIFont;

            fontGeo.Color = hovered ? Color.Yellow : Color.White;
            fontGeo.Height = fontSize;

            var pos = new Vector2(
                Bounds.XStart + padding + textAlign * (Bounds.Width - 2 * padding),
                Bounds.CenterY());

            fontGeo.DrawString(pos, text, textAlign, .5f);
        }
    }
}
