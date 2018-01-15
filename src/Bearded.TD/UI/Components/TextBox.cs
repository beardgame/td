using System.Collections.Generic;
using amulware.Graphics;
using Bearded.TD.Rendering;
using OpenTK;

namespace Bearded.TD.UI.Components
{
    abstract class TextBox<T> : UIComponent
    {
        protected TextBox(Bounds bounds)
            : base(bounds)
        {
        }

        protected abstract IReadOnlyList<T> GetItems();
        protected abstract (string, Color) Format(T item);

        public override void Draw(GeometryManager geometries)
        {
            var entries = GetItems();

            geometries.ConsoleFont.SizeCoefficient = new Vector2(1, 1);
            geometries.ConsoleFont.Height = Constants.UI.FontSize;

            var y = Bounds.Bottom - Constants.UI.LineHeight;
            var i = entries.Count;

            while (y >= -Constants.UI.LineHeight && i > 0)
            {
                (var text, var color) = Format(entries[--i]);
                geometries.ConsoleFont.Color = color;
                geometries.ConsoleFont.DrawString(new Vector2(Bounds.Left, y), text);
                y -= Constants.UI.LineHeight;
            }
        }
    }
}