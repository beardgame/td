using amulware.Graphics;
using Bearded.TD.Rendering;
using OpenTK;

namespace Bearded.TD.UI.Components
{
    class Button : UIComponent
    {
        private const float padding = 8f;

        private readonly string text;
        private readonly float fontSize;
        private readonly float textAlign;

        public Button(Bounds bounds, string text, float fontSize, float textAlign = 0f) : base(bounds)
        {
            this.text = text;
            this.fontSize = fontSize;
            this.textAlign = textAlign;
        }

        public override void Draw(GeometryManager geometries)
        {
            var geo = geometries.UIFont;

            geo.Color = Color.White;
            geo.Height = fontSize;

            var pos = new Vector2(
                Bounds.XStart + padding + textAlign * (Bounds.Width - 2 * padding),
                Bounds.YStart + .5f * Bounds.Height);

            geo.DrawString(pos, text, textAlign, .5f);
        }
    }
}
