using System;
using System.Collections.Generic;
using amulware.Graphics;
using Bearded.TD.Rendering;
using OpenTK;

namespace Bearded.TD.UI.Components
{
    class TextBox<T> : UIComponent
    {
        private readonly Func<IReadOnlyList<T>> itemProvider;
        private readonly Func<T, (string, Color)> itemTransformer;

        public TextBox(Bounds bounds, Func<IReadOnlyList<T>> itemProvider, Func<T, (string, Color)> itemTransformer)
            : base(bounds)
        {
            this.itemProvider = itemProvider;
            this.itemTransformer = itemTransformer;
        }

        public override void Draw(GeometryManager geometries)
        {
            var entries = itemProvider();

            geometries.ConsoleFont.SizeCoefficient = new Vector2(1, 1);
            geometries.ConsoleFont.Height = Constants.UI.FontSize;

            var y = Bounds.YEnd - Constants.UI.LineHeight;
            var i = entries.Count;

            while (y >= -Constants.UI.LineHeight && i > 0)
            {
                (var text, var color) = itemTransformer(entries[--i]);
                geometries.ConsoleFont.Color = color;
                geometries.ConsoleFont.DrawString(new Vector2(Bounds.XStart, y), text);
                y -= Constants.UI.LineHeight;
            }
        }
    }
}