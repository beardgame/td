using System;
using System.Text;
using amulware.Graphics;
using Bearded.TD.Rendering;
using OpenTK;

namespace Bearded.TD.UI.Components
{
    class TextBox : UIComponent
    {
        private readonly Func<(string, Color)> textProvider;
        private readonly bool wrapText;

        public TextBox(Bounds bounds, string text, Color color, bool wrapText = false)
            : this(bounds, () => (text, color), wrapText) { }

        public TextBox(Bounds bounds, Func<(string, Color)> textProvider, bool wrapText = false) : base(bounds)
        {
            this.textProvider = textProvider;
            this.wrapText = wrapText;
        }
        
        public override void Draw(GeometryManager geometries)
        {
            (var text, var color) = textProvider();
            var geo = geometries.ConsoleFont;

            geo.SizeCoefficient = Vector2.One;
            geo.Height = Constants.UI.FontSize;
            geo.Color = color;

            if (!wrapText)
            {
                geo.DrawString(Bounds.TopLeft(), text);
            }
            else
            {
                geo.DrawMultiLineString(Bounds.TopLeft(), splitToWrap(geo, text));
            }
        }

        private string splitToWrap(FontGeometry geo, string text)
        {
            if (geo.StringWidth(text) <= Bounds.Width) return text;

            var words = text.Split(new[] {' '}, StringSplitOptions.RemoveEmptyEntries);
            var sb = new StringBuilder();

            var spaceWidth = geo.StringWidth(" ");
            var currentLineWidth = 0f;

            foreach (var word in words)
            {
                var wordWidth = geo.StringWidth(word);

                // Word is longer than width of the box. Needs its own line.
                if (wordWidth > Bounds.Width)
                {
                    if (currentLineWidth > 0)
                    {
                        sb.Append('\n');
                        currentLineWidth = 0;
                    }
                    sb.Append(word);
                    sb.Append('\n');
                    continue;
                }

                if (currentLineWidth + spaceWidth + wordWidth > Bounds.Width)
                {
                    sb.Append('\n');
                    currentLineWidth = 0;
                }

                sb.Append(word);
                sb.Append(" ");
                currentLineWidth += spaceWidth + wordWidth;
            }

            return sb.ToString();
        }
    }
}
