using System;
using System.Collections.Generic;
using amulware.Graphics;
using Bearded.TD.Rendering;
using OpenTK;

namespace Bearded.TD.UI.Components
{
    class TextBox<T> : UIComponent
    {
        private const float fontSize = 14;
        private const float lineHeight = 16;

        private readonly Func<IReadOnlyList<T>> itemProvider;
        private readonly Func<T, (string, Color)> itemTransformer;

        public TextBox(Bounds bounds, Func<IReadOnlyList<T>> itemProvider, Func<T, (string, Color)> itemTransformer) : base(bounds)
        {
            this.itemProvider = itemProvider;
            this.itemTransformer = itemTransformer;
        }

        public override void Draw(GeometryManager geometries)
        {
            var entries = itemProvider();

            geometries.ConsoleFont.SizeCoefficient = new Vector2(1, 1);
            geometries.ConsoleFont.Height = fontSize;

            var y = Bounds.YEnd - lineHeight;
            var i = entries.Count;

            while (y >= -lineHeight && i > 0)
            {
                (var text, var color) = itemTransformer(entries[--i]);
                geometries.ConsoleFont.Color = color;
                geometries.ConsoleFont.DrawString(new Vector2(Bounds.XStart, y), text);
                y -= lineHeight;
            }
        }
    }
}