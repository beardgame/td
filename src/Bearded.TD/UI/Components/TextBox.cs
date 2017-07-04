using System;
using System.Collections.Generic;
using amulware.Graphics;
using Bearded.TD.Rendering;
using OpenTK;

namespace Bearded.TD.UI.Components
{
    class InjectedTextBox<T> : TextBox<T>
    {
        private readonly Func<IReadOnlyList<T>> itemProvider;
        private readonly Func<T, (string, Color)> itemTransformer;

        public InjectedTextBox(Bounds bounds, Func<IReadOnlyList<T>> itemProvider, Func<T, (string, Color)> itemTransformer)
            : base(bounds)
        {
            this.itemProvider = itemProvider;
            this.itemTransformer = itemTransformer;
        }

        protected override IReadOnlyList<T> GetItems() => itemProvider();

        protected override (string, Color) Format(T item) => itemTransformer(item);
    }

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

            var y = Bounds.YEnd - Constants.UI.LineHeight;
            var i = entries.Count;

            while (y >= -Constants.UI.LineHeight && i > 0)
            {
                (var text, var color) = Format(entries[--i]);
                geometries.ConsoleFont.Color = color;
                geometries.ConsoleFont.DrawString(new Vector2(Bounds.XStart, y), text);
                y -= Constants.UI.LineHeight;
            }
        }
    }
}