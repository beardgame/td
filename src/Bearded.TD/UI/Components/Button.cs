﻿using System;
using amulware.Graphics;
using Bearded.TD.Rendering;
using OpenTK;

namespace Bearded.TD.UI.Components
{
    class Button : FocusableUIComponent
    {
        private const float padding = 8f;
        
        private readonly Action action;
        private readonly string text;
        private readonly float fontSize;
        private readonly float textAlign;

        public Button(Bounds bounds, Action action, string text, float fontSize, float textAlign = 0f) : base(bounds)
        {
            this.action = action;
            this.text = text;
            this.fontSize = fontSize;
            this.textAlign = textAlign;
        }

        public override void HandleInput(InputContext input)
        {
            base.HandleInput(input);
            SetFocus(Bounds.Contains(input.MousePosition));
            if (input.Manager.LeftMouseHit && IsFocused)
                action();
        }

        public override void Draw(GeometryManager geometries)
        {
            var primitivesGeo = geometries.Primitives;

            primitivesGeo.Color = Color.HotPink * (IsFocused ? .2f : .05f);
            primitivesGeo.DrawRectangle(Bounds.XStart, Bounds.YStart, Bounds.Width, Bounds.Height);


            var fontGeo = geometries.UIFont;

            fontGeo.Color = IsFocused ? Color.Yellow : Color.White;
            fontGeo.Height = fontSize;

            var pos = new Vector2(
                Bounds.XStart + padding + textAlign * (Bounds.Width - 2 * padding),
                Bounds.CenterY());

            fontGeo.DrawString(pos, text, textAlign, .5f);
        }
    }
}
